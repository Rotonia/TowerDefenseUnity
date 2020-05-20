using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Services.Interfaces;
using UnityEngine;
using Zenject;

namespace Data
{
    public class DataLoader: IGameDataProvider, IInitializable
    {
        private readonly Dictionary<Type, Dictionary<int, GameDataBase>> _gameDataDict = new Dictionary<Type, Dictionary<int, GameDataBase>>();
        
        public void Initialize()
        {
            LoadData();
        }

        public T[] GetDataByType<T>() where T : GameDataBase
        {
            _gameDataDict.TryGetValue(typeof(T), out var datas);
            if (datas == null)
            {
                return null;
            }
            T[] retArray = new T[datas.Count];

            int i = 0;
            foreach(var data in datas)
            {
                retArray[i] = data.Value as T;
                i++;
            }
            
            return retArray;
        }

        public void LoadData()
        {
            List<Type> dataTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x=> typeof(GameDataBase).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .ToList();
            
            MethodInfo method = typeof(DataLoader)
                .GetMethods()
                .Single(m => m.Name == "LoadAllDataOfType" && m.IsGenericMethodDefinition);

            foreach (Type dataType in dataTypes)
            {
                MethodInfo genericMethod = method.MakeGenericMethod(dataType);
                genericMethod.Invoke(this, null);
            }
        }

        public void LoadAllDataOfType<T>() where T : GameDataBase
        {
            Type dataType = typeof(T);
            string typeName = typeof(T).Name;
            T[] dataObjs = Resources.LoadAll<T>("Data/" + typeName);
            var dataTypeDict = new Dictionary<int, GameDataBase>();
            foreach (T data in dataObjs)
            {
                if (dataTypeDict.TryGetValue(data.Id, out var existingData))
                {
                    Debug.LogError("Duplicate Id's for type: " + typeName + " id: " + data.Id);
                    continue;
                }

                dataTypeDict[data.Id] = data;
            }
            _gameDataDict[dataType] = dataTypeDict;
        }

        public T GetDataById<T>(int id) where T:GameDataBase
        {
            Type dataType = typeof(T);
            if (_gameDataDict.TryGetValue(dataType, out var typeDataDict))
            {
                if (typeDataDict.TryGetValue(id, out var data))
                {
                    return (T)data;
                }
                
                Debug.LogError("No data found for Type: " + dataType.Name + " Id: " + id.ToString());
            }
            else
            {
                Debug.LogError("No data found for Type: " + dataType.Name);
            }

            return default;
        }

       
    }
}