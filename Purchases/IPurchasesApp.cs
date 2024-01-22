﻿using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.Purchases
{
    internal interface IPurchasesApp : IGameSDKService
    {
        Task Initialize();
        Task<(bool, Product[])> GetCatalog();
        Task<(bool, ProductPurchase)> Purchase(string id, string developerPayload);
        Task<ProductPurchase[]> GetPurchases();
        Task<bool> Consume(ProductPurchase productPurchase);
    }
}