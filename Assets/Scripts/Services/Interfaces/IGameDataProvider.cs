using Data;

namespace Services.Interfaces
{
    public interface IGameDataProvider
    {
        T GetDataById<T>(int id) where T : GameDataBase;
        T[] GetDataByType<T>() where T : GameDataBase;
        void LoadData();
    }
}