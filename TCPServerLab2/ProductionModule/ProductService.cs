using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TCPServer.balanceModule;

using TCPServer.ProductionModule;

namespace TCPServer.ProductionModule
{
    public  class ProductService
    {
        // ... (Остальные методы без изменений: GetProductSnapshotAsync, GetAllProductsAsync, AddProductAsync, UpdateProductAsync, DeleteProductAsync, GetAllWarehousesAsync, AddWarehouseAsync, UpdateWarehouseAsync, DeleteWarehouseAsync, GetInventoryAsync)
        public static async Task<string> GetProductSnapshotAsync()
        {
            try
            {
                using (var dbContext = new CrmsystemContext())
                {
                    var products = await dbContext.Products.ToListAsync();
                    var categories = await dbContext.ProductCategories.ToListAsync();
                    var warehouses = await dbContext.Warehouses.ToListAsync();
                    var inventory = await dbContext.Inventories
                        .Include(i => i.Product)
                        .ToListAsync();

                    var categorySummaries = await dbContext.Inventories
                        .Include(i => i.Product)
                        .ThenInclude(p => p.Category)
                        .GroupBy(i => i.Product.Category.Name)
                        .Select(g => new CategorySummaryDto
                        {
                            CategoryName = g.Key,
                            TotalQuantity = g.Sum(i => i.Quantity),
                            TotalValue = g.Sum(i => i.Quantity * i.Product.PurchasePrice)
                        })
                        .ToListAsync();

                    var summary = new ProductSummaryDto
                    {
                        TotalProducts = products.Count,
                        TotalCategories = categories.Count,
                        TotalWarehouses = warehouses.Count,
                        TotalQuantity = inventory.Sum(i => i.Quantity),
                        TotalInventoryValue = inventory.Sum(i => i.Quantity * i.Product.PurchasePrice),
                        Categories = categorySummaries
                    };

                    return JsonConvert.SerializeObject(summary);
                }
            }
            catch (Exception ex)
            {
                return $"Error: Не удалось загрузить сводку по продукции: {ex.Message}";
            }
        }

        public static async Task<string> GetAllProductsAsync()
        {
            try
            {
                using (var dbContext = new CrmsystemContext())
                {
                    var products = await dbContext.Products
                        .Include(p => p.Category)
                        .Include(p => p.Description)
                        .Select(p => new ProductDto
                        {
                            Id = p.ProductId,
                            Name = p.Name,
                            CategoryId = p.CategoryId,
                            PurchasePrice = p.PurchasePrice,
                            SellingPrice = p.SellingPrice,
                            Description = p.Description != null ? p.Description.Content : null
                        })
                        .ToListAsync();

                    return JsonConvert.SerializeObject(products);
                }
            }
            catch (Exception ex)
            {
                return $"Error: Не удалось загрузить продукты: {ex.Message}";
            }
        }

        public static async Task<string> AddProductAsync(string jsonData)
        {
            try
            {
                var productData = JsonConvert.DeserializeObject<ProductDto>(jsonData);
                if (productData == null)
                {
                    return "Error: Неверный формат данных продукта";
                }

                using (var dbContext = new CrmsystemContext())
                {
                    // Проверяем существование категории
                    var category = await dbContext.ProductCategories.FindAsync(productData.CategoryId);
                    if (category == null)
                    {
                        return "Error: Категория не найдена";
                    }

                    // Создаём описание, если есть
                    int? descriptionId = null;
                    if (!string.IsNullOrEmpty(productData.Description))
                    {
                        var description = new Description { Content = productData.Description };
                        dbContext.Descriptions.Add(description);
                        await dbContext.SaveChangesAsync();
                        descriptionId = description.Id;
                    }

                    // Создаём продукт
                    var product = new Product
                    {
                        Name = productData.Name,
                        CategoryId = productData.CategoryId,
                        PurchasePrice = productData.PurchasePrice,
                        SellingPrice = productData.SellingPrice,
                        DescriptionId = descriptionId
                    };

                    dbContext.Products.Add(product);
                    await dbContext.SaveChangesAsync();

                    // Интеграция с балансом: создаём актив, если указано начальное количество
                    if (productData.InitialQuantity.HasValue && productData.InitialQuantity.Value > 0)
                    {
                        var asset = new Asset
                        {
                            Category = "ТМЦ",
                            Name = product.Name,
                            Amount = productData.PurchasePrice * productData.InitialQuantity.Value,
                            Currency = "RUB",
                            AcquisitionDate = DateTime.Now,
                            DescriptionId = descriptionId
                        };
                        dbContext.Assets.Add(asset);

                        // Создаём партию и инвентарь
                        var batch = new ProductBatch
                        {
                            ProductId = product.ProductId,
                            Quantity = productData.InitialQuantity.Value
                        };
                        dbContext.ProductBatches.Add(batch);

                        var inventory = new Inventory
                        {
                            ProductId = product.ProductId,
                            WarehouseId = 1, // По умолчанию первый склад
                            Quantity = productData.InitialQuantity.Value,
                            ReservedQuantity = 0
                        };
                        dbContext.Inventories.Add(inventory);

                        // Логирование
                        var auditLog = new AuditLog
                        {
                            UserName = "System",
                            Action = "Создание",
                            EntityType = "Asset",
                            EntityId = product.ProductId,
                            Details = $"Добавлен актив ТМЦ: {product.Name}, Сумма: {asset.Amount}",
                            Timestamp = DateTime.Now
                        };
                        dbContext.AuditLogs.Add(auditLog);

                        await dbContext.SaveChangesAsync();
                    }

                    // Логирование добавления продукта
                    var productAuditLog = new AuditLog
                    {
                        UserName = "System",
                        Action = "Создание",
                        EntityType = "Product",
                        EntityId = product.ProductId,
                        Details = $"Добавлен продукт: {product.Name}, Цена закупки: {product.PurchasePrice}",
                        Timestamp = DateTime.Now
                    };
                    dbContext.AuditLogs.Add(productAuditLog);
                    await dbContext.SaveChangesAsync();

                    return JsonConvert.SerializeObject(new { Success = true, ProductId = product.ProductId });
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Success = false, Error = ex.Message });
            }
        }
        public  async Task<List<ProductCategory>> GetCategoriesAsync()
        {
            using var context = new CrmsystemContext();
            return await context.ProductCategories.ToListAsync();
        }

        public  async Task<List<Product>> GetProductsAsync()
        {
            using var context = new CrmsystemContext();
            return await context.Products
                .Include(p => p.Category)
                .Include(p => p.Description)
                .ToListAsync();
        }

        public  async Task<List<AuditLog>> GetAuditLogsAsync()
        {
            using var context = new CrmsystemContext();
            return await context.AuditLogs.ToListAsync();
        }
        public static async Task<string> UpdateProductAsync(string jsonData)
        {
            try
            {
                var productData = JsonConvert.DeserializeObject<ProductDto>(jsonData);
                if (productData == null)
                {
                    return "Error: Неверный формат данных продукта";
                }

                using (var dbContext = new CrmsystemContext())
                {
                    var product = await dbContext.Products
                        .Include(p => p.Description)
                        .FirstOrDefaultAsync(p => p.ProductId == productData.Id);

                    if (product == null)
                    {
                        return "Error: Продукт не найден";
                    }

                    // Проверяем категорию
                    var category = await dbContext.ProductCategories.FindAsync(productData.CategoryId);
                    if (category == null)
                    {
                        return "Error: Категория не найдена";
                    }

                    // Обновляем описание
                    if (product.Description == null && !string.IsNullOrEmpty(productData.Description))
                    {
                        product.Description = new Description { Content = productData.Description };
                        dbContext.Descriptions.Add(product.Description);
                    }
                    else if (product.Description != null)
                    {
                        product.Description.Content = productData.Description;
                    }

                    // Обновляем продукт
                    product.Name = productData.Name;
                    product.CategoryId = productData.CategoryId;
                    product.PurchasePrice = productData.PurchasePrice;
                    product.SellingPrice = productData.SellingPrice;

                    // Логирование
                    var auditLog = new AuditLog
                    {
                        UserName = "System",
                        Action = "Обновление",
                        EntityType = "Product",
                        EntityId = product.ProductId,
                        Details = $"Обновлён продукт: {product.Name}, Новая цена закупки: {product.PurchasePrice}",
                        Timestamp = DateTime.Now
                    };
                    dbContext.AuditLogs.Add(auditLog);

                    await dbContext.SaveChangesAsync();

                    return JsonConvert.SerializeObject(new { Success = true });
                }
            }
            catch (Exception ex)
            {
                return $"Error: Не удалось обновить продукт: {ex.Message}";
            }
        }

        public static async Task<string> DeleteProductAsync(string jsonData)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<dynamic>(jsonData);
                int productId = data.Id;

                using (var dbContext = new CrmsystemContext())
                {
                    var product = await dbContext.Products
                        .Include(p => p.Description)
                        .FirstOrDefaultAsync(p => p.ProductId == productId);

                    if (product == null)
                    {
                        return "Error: Продукт не найден";
                    }

                    // Удаляем связанные активы
                    var assets = await dbContext.Assets
                        .Where(a => a.Name == product.Name && a.Category == "ТМЦ")
                        .ToListAsync();
                    dbContext.Assets.RemoveRange(assets);

                    // Удаляем описание
                    if (product.Description != null)
                    {
                        dbContext.Descriptions.Remove(product.Description);
                    }

                    // Удаляем инвентарь и партии
                    var inventories = await dbContext.Inventories
                        .Where(i => i.ProductId == productId)
                        .ToListAsync();
                    dbContext.Inventories.RemoveRange(inventories);

                    var batches = await dbContext.ProductBatches
                        .Where(b => b.ProductId == productId)
                        .ToListAsync();
                    dbContext.ProductBatches.RemoveRange(batches);

                    // Логирование
                    var auditLog = new AuditLog
                    {
                        UserName = "System",
                        Action = "Удаление",
                        EntityType = "Product",
                        EntityId = product.ProductId,
                        Details = $"Удалён продукт: {product.Name}",
                        Timestamp = DateTime.Now
                    };
                    dbContext.AuditLogs.Add(auditLog);

                    dbContext.Products.Remove(product);
                    await dbContext.SaveChangesAsync();

                    return JsonConvert.SerializeObject(new { Success = true });
                }
            }
            catch (Exception ex)
            {
                return $"Error: Не удалось удалить продукт: {ex.Message}";
            }
        }

        public static async Task<string> GetAllWarehousesAsync()
        {
            try
            {
                using (var dbContext = new CrmsystemContext())
                {
                    var warehouses = await dbContext.Warehouses
                        .Select(w => new WarehouseDto
                        {
                            Id = w.Id,
                            Name = w.Name
                        })
                        .ToListAsync();

                    return JsonConvert.SerializeObject(warehouses);
                }
            }
            catch (Exception ex)
            {
                return $"Error: Не удалось загрузить склады: {ex.Message}";
            }
        }

        public static async Task<string> AddWarehouseAsync(string jsonData)
        {
            try
            {
                var warehouseData = JsonConvert.DeserializeObject<WarehouseDto>(jsonData);
                if (warehouseData == null)
                {
                    return "Error: Неверный формат данных склада";
                }

                using (var dbContext = new CrmsystemContext())
                {
                    var warehouse = new Warehouse
                    {
                        Name = warehouseData.Name
                    };

                    dbContext.Warehouses.Add(warehouse);
                    await dbContext.SaveChangesAsync();

                    // Логирование
                    var auditLog = new AuditLog
                    {
                        UserName = "System",
                        Action = "Создание",
                        EntityType = "Warehouse",
                        EntityId = warehouse.Id,
                        Details = $"Добавлен склад: {warehouse.Name}",
                        Timestamp = DateTime.Now
                    };
                    dbContext.AuditLogs.Add(auditLog);
                    await dbContext.SaveChangesAsync();

                    return JsonConvert.SerializeObject(new { Success = true, WarehouseId = warehouse.Id });
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Success = false, Error = ex.Message });
            }
        }

        public static async Task<string> UpdateWarehouseAsync(string jsonData)
        {
            try
            {
                var warehouseData = JsonConvert.DeserializeObject<WarehouseDto>(jsonData);
                if (warehouseData == null)
                {
                    return "Error: Неверный формат данных склада";
                }

                using (var dbContext = new CrmsystemContext())
                {
                    var warehouse = await dbContext.Warehouses
                        .FirstOrDefaultAsync(w => w.Id == warehouseData.Id);

                    if (warehouse == null)
                    {
                        return "Error: Склад не найден";
                    }

                    warehouse.Name = warehouseData.Name;

                    // Логирование
                    var auditLog = new AuditLog
                    {
                        UserName = "System",
                        Action = "Обновление",
                        EntityType = "Warehouse",
                        EntityId = warehouse.Id,
                        Details = $"Обновлён склад: {warehouse.Name}",
                        Timestamp = DateTime.Now
                    };
                    dbContext.AuditLogs.Add(auditLog);

                    await dbContext.SaveChangesAsync();

                    return JsonConvert.SerializeObject(new { Success = true });
                }
            }
            catch (Exception ex)
            {
                return $"Error: Не удалось обновить склад: {ex.Message}";
            }
        }

        public static async Task<string> DeleteWarehouseAsync(string jsonData)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<dynamic>(jsonData);
                int warehouseId = data.Id;

                using (var dbContext = new CrmsystemContext())
                {
                    var warehouse = await dbContext.Warehouses
                        .FirstOrDefaultAsync(w => w.Id == warehouseId);

                    if (warehouse == null)
                    {
                        return "Error: Склад не найден";
                    }

                    // Проверяем, есть ли инвентарь на складе
                    var inventory = await dbContext.Inventories
                        .AnyAsync(i => i.WarehouseId == warehouseId);
                    if (inventory)
                    {
                        return "Error: Нельзя удалить склад с остатками";
                    }

                    // Логирование
                    var auditLog = new AuditLog
                    {
                        UserName = "System",
                        Action = "Удаление",
                        EntityType = "Warehouse",
                        EntityId = warehouse.Id,
                        Details = $"Удалён склад: {warehouse.Name}",
                        Timestamp = DateTime.Now
                    };
                    dbContext.AuditLogs.Add(auditLog);

                    dbContext.Warehouses.Remove(warehouse);
                    await dbContext.SaveChangesAsync();

                    return JsonConvert.SerializeObject(new { Success = true });
                }
            }
            catch (Exception ex)
            {
                return $"Error: Не удалось удалить склад: {ex.Message}";
            }
        }

        public static async Task<string> GetInventoryAsync()
        {
            try
            {
                using (var dbContext = new CrmsystemContext())
                {
                    var inventory = await dbContext.Inventories
                        .Include(i => i.Product)
                        .Include(i => i.Warehouse)
                        .Select(i => new InventoryDto
                        {
                            Id = i.Id,
                            ProductId = i.ProductId,
                            WarehouseId = i.WarehouseId,
                            Quantity = i.Quantity,
                            ReservedQuantity = i.ReservedQuantity
                        })
                        .ToListAsync();

                    return JsonConvert.SerializeObject(inventory);
                }
            }
            catch (Exception ex)
            {
                return $"Error: Не удалось загрузить инвентарь: {ex.Message}";
            }
        }
        public static async Task<string> AddInventoryTransactionAsync(string jsonData)
        {
            try
            {
                var transactionData = JsonConvert.DeserializeObject<InventoryTransactionDto>(jsonData);
                if (transactionData == null)
                {
                    return "Error: Неверный формат данных транзакции";
                }

                using (var dbContext = new CrmsystemContext())
                {
                    
                    var product = await dbContext.Products.FindAsync(transactionData.ProductBatchId);
                    if (product == null)
                    {
                        return "Error: Продукт не найден";
                    }

                    
                    var batch = await dbContext.ProductBatches
                        .FirstOrDefaultAsync(b => b.ProductId == transactionData.ProductBatchId);
                    if (batch == null)
                    {
                        batch = new ProductBatch
                        {
                            ProductId = transactionData.ProductBatchId,
                            Quantity = 0
                        };
                        dbContext.ProductBatches.Add(batch);
                    }

                   var transactionType = transactionData.TransactionType?.ToLower();
                    if (!new[] { "receipt", "shipment", "transfer" }.Contains(transactionType))
                    {
                        return "Error: Неверный тип транзакции";
                    }

                    if (transactionType == "receipt")
                    {
                        
                        if (!transactionData.ToWarehouseId.HasValue)
                        {
                            return "Error: Укажите склад назначения";
                        }
                        var warehouse = await dbContext.Warehouses.FindAsync(transactionData.ToWarehouseId.Value);
                        if (warehouse == null)
                        {
                            return "Error: Склад не найден";
                        }

                        
                        var inventory = await dbContext.Inventories
                            .FirstOrDefaultAsync(i => i.ProductId == transactionData.ProductBatchId && i.WarehouseId == transactionData.ToWarehouseId.Value);
                        if (inventory == null)
                        {
                            inventory = new Inventory
                            {
                                ProductId = transactionData.ProductBatchId,
                                WarehouseId = transactionData.ToWarehouseId.Value,
                                Quantity = transactionData.Quantity,
                                ReservedQuantity = 0
                            };
                            dbContext.Inventories.Add(inventory);
                        }
                        else
                        {
                            inventory.Quantity += transactionData.Quantity;
                        }

                        batch.Quantity += transactionData.Quantity;

                       
                        var asset = new Asset
                        {
                            Category = "ТМЦ",
                            Name = product.Name,
                            Amount = transactionData.Quantity * product.PurchasePrice,
                            Currency = "RUB",
                            AcquisitionDate = DateTime.Now
                        };
                        dbContext.Assets.Add(asset);

                     
                        var assetAuditLog = new AuditLog
                        {
                            UserName = "System",
                            Action = "Создание",
                            EntityType = "Asset",
                            EntityId = product.ProductId,
                            Details = $"Добавлен актив ТМЦ: {product.Name}, Сумма: {asset.Amount}",
                            Timestamp = DateTime.Now
                        };
                        dbContext.AuditLogs.Add(assetAuditLog);
                    }
                    else if (transactionType == "shipment")
                    {
                       
                        if (!transactionData.FromWarehouseId.HasValue)
                        {
                            return "Error: Укажите склад отправления";
                        }
                        var warehouse = await dbContext.Warehouses.FindAsync(transactionData.FromWarehouseId.Value);
                        if (warehouse == null)
                        {
                            return "Error: Склад не найден";
                        }

                  
                        var inventory = await dbContext.Inventories
                            .FirstOrDefaultAsync(i => i.ProductId == transactionData.ProductBatchId && i.WarehouseId == transactionData.FromWarehouseId.Value);
                        if (inventory == null || inventory.Quantity < transactionData.Quantity)
                        {
                            return "Error: Недостаточно продукции на складе";
                        }

                        inventory.Quantity -= transactionData.Quantity;
                        batch.Quantity -= transactionData.Quantity;

                        var asset = await dbContext.Assets
                            .FirstOrDefaultAsync(a => a.Name == product.Name && a.Category == "ТМЦ");
                        if (asset != null)
                        {
                            asset.Amount -= transactionData.Quantity * product.PurchasePrice;
                            if (asset.Amount <= 0)
                            {
                                dbContext.Assets.Remove(asset);
                            }
                        }

                        var assetAuditLog = new AuditLog
                        {
                            UserName = "System",
                            Action = "Обновление",
                            EntityType = "Asset",
                            EntityId = product.ProductId,
                            Details = $"Уменьшен актив ТМЦ: {product.Name}, Сумма: {transactionData.Quantity * product.PurchasePrice}",
                            Timestamp = DateTime.Now
                        };
                        dbContext.AuditLogs.Add(assetAuditLog);
                    }
                    else if (transactionType == "transfer")
                    {
                      
                        if (!transactionData.FromWarehouseId.HasValue || !transactionData.ToWarehouseId.HasValue)
                        {
                            return "Error: Укажите склады отправления и назначения";
                        }
                        var fromWarehouse = await dbContext.Warehouses.FindAsync(transactionData.FromWarehouseId.Value);
                        var toWarehouse = await dbContext.Warehouses.FindAsync(transactionData.ToWarehouseId.Value);
                        if (fromWarehouse == null || toWarehouse == null)
                        {
                            return "Error: Склад не найден";
                        }

                        var fromInventory = await dbContext.Inventories
                            .FirstOrDefaultAsync(i => i.ProductId == transactionData.ProductBatchId && i.WarehouseId == transactionData.FromWarehouseId.Value);
                        if (fromInventory == null || fromInventory.Quantity < transactionData.Quantity)
                        {
                            return "Error: Недостаточно продукции на складе отправления";
                        }

                        fromInventory.Quantity -= transactionData.Quantity;

                 
                       var toInventory = await dbContext.Inventories
                           .FirstOrDefaultAsync(i => i.ProductId == transactionData.ProductBatchId && i.WarehouseId == transactionData.ToWarehouseId.Value);
                        if (toInventory == null)
                        {
                            toInventory = new Inventory
                            {
                                ProductId = transactionData.ProductBatchId,
                                WarehouseId = transactionData.ToWarehouseId.Value,
                                Quantity = transactionData.Quantity,
                                ReservedQuantity = 0
                            };
                            dbContext.Inventories.Add(toInventory);
                        }
                        else
                        {
                            toInventory.Quantity += transactionData.Quantity;
                        }
                    }

                 
                    var transaction = new InventoryTransaction
                    {
                        ProductBatchId = transactionData.ProductBatchId,
                        FromWarehouseId = transactionData.FromWarehouseId, // Nullable, так как может быть null
                        ToWarehouseId = transactionData.ToWarehouseId,     // Nullable, так как может быть null
                        Quantity = transactionData.Quantity,
                        TransactionType = transactionData.TransactionType, // Сохраняем как строку
                        TransactionDate = DateTime.Now
                    };
                    dbContext.InventoryTransactions.Add(transaction);

                    // Логирование транзакции
                    var transactionAuditLog = new AuditLog
                    {
                        UserName = "System",
                        Action = "Создание",
                        EntityType = "InventoryTransaction",
                        EntityId = transaction.Id,
                        Details = $"Выполнена транзакция: {transactionData.TransactionType}, Продукт: {product.Name}, Количество: {transactionData.Quantity}",
                        Timestamp = DateTime.Now
                    };
                    dbContext.AuditLogs.Add(transactionAuditLog);

                    await dbContext.SaveChangesAsync();

                    return JsonConvert.SerializeObject(new { Success = true, TransactionId = transaction.Id });
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Success = false, Error = ex.Message });
            }
        }

        public static async Task<string> CompareProductSnapshotsAsync(string jsonData)
        {
            try
            {
                var periods = JsonConvert.DeserializeObject<dynamic>(jsonData);
                if (periods == null)
                {
                    return "Error: Неверный формат данных периодов";
                }

                DateTime period1Start = DateTime.Parse((string)periods.Period1Start);
                DateTime period1End = DateTime.Parse((string)periods.Period1End);
                DateTime period2Start = DateTime.Parse((string)periods.Period2Start);
                DateTime period2End = DateTime.Parse((string)periods.Period2End);

                using (var dbContext = new CrmsystemContext())
                {
                    var period1Inventory = await dbContext.InventoryTransactions
                        .Include(t => t.ProductBatch)
                        .ThenInclude(b => b.Product)
                        .Where(t => t.TransactionDate >= period1Start && t.TransactionDate <= period1End)
                        .ToListAsync();

                    var period2Inventory = await dbContext.InventoryTransactions
                        .Include(t => t.ProductBatch)
                        .ThenInclude(b => b.Product)
                        .Where(t => t.TransactionDate >= period2Start && t.TransactionDate <= period2End)
                        .ToListAsync();

                    var period1Summary = new
                    {
                        Quantity = period1Inventory.Sum(t => t.TransactionType.ToLower() == "receipt" ? t.Quantity : t.TransactionType.ToLower() == "shipment" ? -t.Quantity : 0),
                        Value = period1Inventory.Sum(t => t.TransactionType.ToLower() == "receipt" ? t.Quantity * t.ProductBatch.Product.PurchasePrice : t.TransactionType.ToLower() == "shipment" ? -t.Quantity * t.ProductBatch.Product.PurchasePrice : 0)
                    };

                    var period2Summary = new
                    {
                        Quantity = period2Inventory.Sum(t => t.TransactionType.ToLower() == "receipt" ? t.Quantity : t.TransactionType.ToLower() == "shipment" ? -t.Quantity : 0),
                        Value = period2Inventory.Sum(t => t.TransactionType.ToLower() == "receipt" ? t.Quantity * t.ProductBatch.Product.PurchasePrice : t.TransactionType.ToLower() == "shipment" ? -t.Quantity * t.ProductBatch.Product.PurchasePrice : 0)
                    };

                    var comparisonResult = new
                    {
                        Period1 = period1Summary,
                        Period2 = period2Summary
                    };

                    return JsonConvert.SerializeObject(comparisonResult);
                }
            }
            catch (Exception ex)
            {
                return $"Error: Не удалось сравнить снимки продукции: {ex.Message}";
            }
        }
    }
}