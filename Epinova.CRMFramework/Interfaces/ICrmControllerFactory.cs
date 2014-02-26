
namespace Epinova.CRMFramework.Interfaces
{
    public interface ICrmControllerFactory
    {
        CrmEntityController<T> GetEntityController<T>() where T : class, new();

        CrmManyToManyRelationshipController<T, V> GetManyToManyRelationshipController<T, V>()
            where T : class, new()
            where V : class, new();
    }
}
