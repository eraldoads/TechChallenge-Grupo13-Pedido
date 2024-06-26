namespace Application.Interfaces
{
    public interface IPedidoScopedService
    {
        Task DoWork(CancellationToken cancellationToken);
    }
}
