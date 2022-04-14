using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
using Exercise17.FuncApi.Models;
using Exercise17.FuncApi.Helpers;
using System.Linq;
using Exercise17.Shared;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Azure.Storage.Blob;

namespace Exercise17.FuncApi
{
    public static class Api
    {
        [FunctionName("Get")]
        public static async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "machines")] HttpRequest req,
            [Table("machinepark", Connection = "AzureWebJobsStorage")] CloudTable table,
            ILogger log)
        {
            log.LogInformation("Getting all items...");

            var query = new TableQuery<MachineEntity>();
            var res = await table.ExecuteQuerySegmentedAsync(query, null);
            var result = res.Select(Mappers.ToMachine).ToList();

            return new OkObjectResult(result);
        }

        [FunctionName("GetDetails")]
        public static async Task<IActionResult> Details(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route ="machines/{id}")] HttpRequest req,
            [Table("individualmachines", Connection = "AzureWebJobsStorage")] CloudTable machine,
            string id,
            ILogger log)
        {
            
            log.LogInformation("Details about machine is collected...");
            var query = new TableQuery<MachineEntity>();
            var res = await machine.ExecuteQuerySegmentedAsync(query, null);
 
            var result = res.Select(Mappers.ToMachineDetails).ToList();
            var selected = result.Where(m => m.Id == id).ToList();
            return new OkObjectResult(selected);
        }

        [FunctionName("Create")]
        public static async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "machines")] HttpRequest req,
            [Table("machinepark", Connection = "AzureWebJobsStorage")] IAsyncCollector<MachineEntity> machines,
            ILogger log)
        {
            log.LogInformation("Creating new machine");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var createMachine = JsonConvert.DeserializeObject<CreateMachineDto>(requestBody);

            if (createMachine == null) return new BadRequestResult();

            var machine = new Machine
            {
                Name = createMachine.Name,
                Online = false
            };

            await machines.AddAsync(machine.ToMachineEntity());

            return new OkObjectResult(machine);
        }

        [FunctionName("UpdateData")]
        public static async Task<IActionResult> UpdateData(
             [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "machines/data/{id}")] HttpRequest req,
             [Table("machinepark", Connection = "AzureWebJobsStorage")] CloudTable machinePark,
             [Table("individualmachines", Connection = "AzureWebJobsStorage")] CloudTable individual,
             string id,
             ILogger log)
        {
            log.LogInformation("Updating machine");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var machineToUpdate = JsonConvert.DeserializeObject<Machine>(requestBody);

            if (machineToUpdate is null || machineToUpdate.Id != id) return new BadRequestResult();

            var machineEntity = new MachineEntity
            {
                Id = machineToUpdate.Id,
                Name = machineToUpdate.Name,
                Online = machineToUpdate.Online,
                Data = machineToUpdate.Data,
                RowKey = DateTime.Now.ToString("G"),
                PartitionKey = machineToUpdate.Id
            };

            var machineUpdate = machineToUpdate.ToMachineEntity();
            machineUpdate.ETag = "*";

            var operation = TableOperation.Replace(machineUpdate);
            await machinePark.ExecuteAsync(operation);

            await individual.ExecuteAsync(TableOperation.Insert(machineEntity));

            return new OkObjectResult(machineEntity);
        }

        [FunctionName("UpdateStatus")]
        public static async Task<IActionResult> UpdateStatus(
             [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "machines/status/{id}")] HttpRequest req,
             [Table("machinepark", Connection = "AzureWebJobsStorage")] CloudTable machinePark,
             string id,
             ILogger log)
        {
            log.LogInformation("Updating machine status");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var machineToUpdate = JsonConvert.DeserializeObject<Machine>(requestBody);

            if (machineToUpdate is null || machineToUpdate.Id != id) return new BadRequestResult();

            var machineUpdate = machineToUpdate.ToMachineEntity();
            machineUpdate.ETag = "*";

            var operation = TableOperation.Replace(machineUpdate);
            await machinePark.ExecuteAsync(operation);

            return new NoContentResult();
        }

        [FunctionName("Delete")]
        public static async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "machines/{id}")] HttpRequest req,
            [Table("machinepark", "Machines", "{id}", Connection = "AzureWebJobsStorage")] MachineEntity machineTableToRemove,
            [Table("machinepark", Connection = "AzureWebJobsStorage")] CloudTable machineTable,
            [Queue("machinequeue", Connection = "AzureWebJobsStorage")] IAsyncCollector<MachineEntity> queueMachine,
            string id,
            ILogger log)
        {
            log.LogInformation("Deleting a machine");

            if (machineTableToRemove == null) return new BadRequestResult();

            await queueMachine.AddAsync(machineTableToRemove);

            var operation = TableOperation.Delete(machineTableToRemove);
            var res = await machineTable.ExecuteAsync(operation);

            return new NoContentResult();
        }

        [FunctionName("GetRemovedFromQueue")]
        public static async Task RemoveFromQueue(
            [QueueTrigger("machinequeue", Connection = "AzureWebJobsStorage")] MachineEntity machine,
            [Blob("removed", Connection = "AzureWebJobsStorage")] CloudBlobContainer blobContainer,
            ILogger log)
        {
            log.LogInformation("Archiving has started");
            await blobContainer.CreateIfNotExistsAsync();
            var blob = blobContainer.GetBlockBlobReference($"{machine.Id}.txt");
            await blob.UploadTextAsync($"Machine name: {machine.Name}\nMachine id: {machine.Id} \nDeleted time: {machine.Timestamp:g} \nLast mesured temperature: {machine.Data}");
        }
    }
}

