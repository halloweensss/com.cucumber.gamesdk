﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using GameSDK.Core;
using GameSDK.Core.Properties;
using GameSDK.Purchases;
using UnityEngine;

namespace GameSDK.Plugins.YaGames.Purchases
{
    public class YaPurchases : IPurchasesApp
    {
        private static readonly YaPurchases _instance = new YaPurchases();

        private Product[] _productsReceived;
        private ProductPurchase _productsPurchase;
        private ProductPurchase[] _productPurchases;
        
        private PurchaseStatus _statusResponse = PurchaseStatus.None;

        private InitializationStatus _status = InitializationStatus.None;
        public PlatformServiceType PlatformService => PlatformServiceType.YaGames;
        public InitializationStatus InitializationStatus => _status;
        public async Task Initialize()
        {
#if !UNITY_EDITOR
            YaPurchasesInitialize(OnSuccess, OnError);
            _status = InitializationStatus.Waiting;
            
            while (_status == InitializationStatus.Waiting)
                await Task.Yield();
#else
            _status = InitializationStatus.Waiting;
            OnSuccess();
            await Task.CompletedTask;
#endif

            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                _instance._status = InitializationStatus.Initialized;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Purchases]: YaGamesApp initialized!");
                }
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                _instance._status = InitializationStatus.Error;
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Purchases]: An error occurred while initializing the YaGamesApp!");
                }
            }
        }

        public async Task<(bool, Product[])> GetCatalog()
        {
#if !UNITY_EDITOR
            YaPurchasesGetCatalog(OnSuccess, OnError);
            _statusResponse = PurchaseStatus.Waiting;
            
            while (_statusResponse == PurchaseStatus.Waiting)
                await Task.Yield();
#else
            _statusResponse = PurchaseStatus.Waiting;
            var products = new YaProduct[]
            {
                new()
                {
                    id = "test_pack_1",
                    title = "+100 Coins",
                    description = "+100 Coins",
                    price = "0.01 YAN",
                    priceCurrencyCode = "YAN",
                    priceValue = "0.01"
                },
                new()
                {
                    id = "test_pack_2",
                    title = "NoAds",
                    description = "NoAds",
                    price = "0.01 YAN",
                    priceCurrencyCode = "YAN",
                    priceValue = "0.01"
                }
            };
            
            var catalog = new YaCatalog()
            {
                products = products
            };
            OnSuccess(JsonUtility.ToJson(catalog));
            await Task.CompletedTask;
#endif

            return (_statusResponse == PurchaseStatus.Success, _productsReceived);
            
            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnSuccess(string data)
            {
                var products = JsonUtility.FromJson<YaCatalog>(data).products;

                _instance._productsReceived = new Product[products.Length];

                for(int i = 0; i < products.Length; i++)
                {
                    var product = products[i];

                    _instance._productsReceived[i] = new Product()
                    {
                        Id = product.id,
                        Title = product.title,
                        Description = product.description,
                        Price = product.price,
                        PriceCurrencyCode = product.priceCurrencyCode,
                        PriceValue = product.priceValue
                    };
                }
                
                _instance._statusResponse = PurchaseStatus.Success;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Purchases]: YaGamesApp catalog received!");
                }
            }

            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnError(string data)
            {
                _instance._productsReceived = null;
                _instance._statusResponse = PurchaseStatus.Error;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Purchases]: An error occurred while getting catalog the YaGamesApp\n{data}!");
                }
            }
        }

        public async Task<(bool, ProductPurchase)> Purchase(string id, string developerPayload)
        {
#if !UNITY_EDITOR
            YaPurchasesPurchase(id, developerPayload, OnSuccess, OnError);
            _statusResponse = PurchaseStatus.Waiting;
            
            while (_statusResponse == PurchaseStatus.Waiting)
                await Task.Yield();
#else
            _statusResponse = PurchaseStatus.Waiting;
            OnSuccess(JsonUtility.ToJson(new YaProductPurchase()
            {
                productID = id,
                purchaseToken = $"{id}_token",
                signature = string.Empty,
                developerPayload = developerPayload,
            }));
            await Task.CompletedTask;
#endif

            return (_statusResponse == PurchaseStatus.Success, _productsPurchase);
            
            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnSuccess(string data)
            {
                var product = JsonUtility.FromJson<YaProductPurchase>(data);

                _instance._productsPurchase = new ProductPurchase()
                {
                    Id = product.productID,
                    Payload = product.developerPayload,
                    Signature = product.signature,
                    Token = product.purchaseToken
                };
                
                _instance._statusResponse = PurchaseStatus.Success;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Purchases]: YaGamesApp product purchased!");
                }
            }

            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnError(string data)
            {
                _instance._productsReceived = null;
                _instance._statusResponse = PurchaseStatus.Error;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Purchases]: An error occurred while purchase the YaGamesApp\n{data}!");
                }
            }
        }

        public async Task<ProductPurchase[]> GetPurchases()
        {
#if !UNITY_EDITOR
            YaPurchasesGetPurchases(OnSuccess, OnError);
            _statusResponse = PurchaseStatus.Waiting;
            
            while (_statusResponse == PurchaseStatus.Waiting)
                await Task.Yield();
#else
            _statusResponse = PurchaseStatus.Waiting;

            var purchases = new YaPurchasesCatalog()
            {
                purchases = new YaProductPurchase[]
                {
                    new()
                    {
                        productID = "test_pack_1",
                        developerPayload = "test_pack_1_payload",
                        signature = "test_pack_1_signature",
                        purchaseToken = "test_pack_1_token",
                    },
                    new()
                    {
                        productID = "test_pack_2",
                        developerPayload = "test_pack_2_payload",
                        signature = "test_pack_2_signature",
                        purchaseToken = "test_pack_2_token",
                    },
                }
            };
            
            OnSuccess(JsonUtility.ToJson(purchases));
            await Task.CompletedTask;
#endif

            return _productPurchases;
            
            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnSuccess(string data)
            {
                var purchases = JsonUtility.FromJson<YaPurchasesCatalog>(data)?.purchases;

                if (purchases == null || purchases.Length == 0)
                {
                    _instance._productPurchases = Array.Empty<ProductPurchase>();
                }
                else
                {
                    _instance._productPurchases = new ProductPurchase[purchases.Length];

                    for (int i = 0; i < purchases.Length; i++)
                    {
                        var purchase = purchases[i];
                        _instance._productPurchases[i] = new ProductPurchase()
                        {
                            Id = purchase.productID,
                            Payload = purchase.developerPayload,
                            Signature = purchase.signature,
                            Token = purchase.purchaseToken
                        };
                    }
                }

                _instance._statusResponse = PurchaseStatus.Success;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Purchases]: YaGamesApp purchases received!");
                }
            }

            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnError(string data)
            {
                _instance._productPurchases = Array.Empty<ProductPurchase>();
                _instance._statusResponse = PurchaseStatus.Error;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Purchases]: An error occurred while get purchases the YaGamesApp\n{data}!");
                }
            }
        }

        public async Task<bool> Consume(ProductPurchase productPurchase)
        {
#if !UNITY_EDITOR
            YaPurchasesConsume(productPurchase.Token, OnSuccess, OnError);
            _statusResponse = PurchaseStatus.Waiting;
            
            while (_statusResponse == PurchaseStatus.Waiting)
                await Task.Yield();
#else
            _statusResponse = PurchaseStatus.Waiting;
            OnSuccess();
            
            await Task.CompletedTask;
#endif

            return _statusResponse == PurchaseStatus.Success;
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                _instance._statusResponse = PurchaseStatus.Success;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Purchases]: YaGamesApp product consumed!");
                }
            }

            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnError(string data)
            {
                _instance._statusResponse = PurchaseStatus.Error;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Purchases]: An error occurred while consume purchase the YaGamesApp\n{data}!");
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            GameSDK.Purchases.Purchases.Instance.Register(_instance);
        }
        
        [DllImport("__Internal")]
        private static extern void YaPurchasesInitialize(Action onSuccess, Action onError);
        [DllImport("__Internal")]
        private static extern void YaPurchasesGetCatalog(Action<string> onSuccess, Action<string> onError);
        [DllImport("__Internal")]
        private static extern void YaPurchasesPurchase(string id, string developerPayload, Action<string> onSuccess, Action<string> onError);
        [DllImport("__Internal")]
        private static extern void YaPurchasesGetPurchases(Action<string> onSuccess, Action<string> onError);
        [DllImport("__Internal")]
        private static extern void YaPurchasesConsume(string token, Action onSuccess, Action<string> onError);
    }
}