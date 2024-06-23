using Data.Context;
using Domain.Entities;
using Domain.Entities.Output;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repository
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly MySQLContext _context;
        private readonly MySQLContextCliente _clienteContext;
        private readonly MySQLContextProduto _produtoContext;


        /// <summary>
        /// Inicializa uma nova instância do repositório de pedidos com o contexto fornecido.
        /// </summary>
        /// <param name="context">O contexto MySQL para o repositório de pedidos.</param>
        public PedidoRepository(MySQLContext context, MySQLContextCliente clienteContext, MySQLContextProduto produtoContext)
        {
            _context = context;
            _clienteContext = clienteContext;
            _produtoContext = produtoContext;
        }

        /// <summary>
        /// Obtém todos os pedidos com informações detalhadas dos clientes, combos e produtos do contexto do banco de dados.
        /// </summary>
        /// <returns>Uma lista de pedidos detalhados com informações de cliente, combo e produto.</returns>
        public async Task<List<PedidoOutput>> GetPedidos()
        {
            var pedidosAgrupados = new Dictionary<int, PedidoOutput>();

            var pedidos = await _context.Pedido
                .Include(p => p.Combos)
                    .ThenInclude(c => c.Produtos)
                .Where(p => p.StatusPedido != "Finalizado")
                .ToListAsync();

            foreach (var pedido in pedidos)
            {
                if (!pedidosAgrupados.ContainsKey(pedido.IdPedido))
                {
                    var cliente = await _clienteContext.Cliente.FindAsync(pedido.IdCliente);
                    var pedidoOutput = new PedidoOutput
                    {
                        IdPedido = pedido.IdPedido,
                        DataPedido = pedido.DataPedido,
                        StatusPedido = pedido.StatusPedido,
                        NomeCliente = cliente != null ? $"{cliente.Nome} {cliente.Sobrenome}" : "Cliente não encontrado",
                        ValorTotalPedido = pedido.ValorTotal,
                        Combo = new List<ComboOutput>()
                    };

                    pedidosAgrupados[pedido.IdPedido] = pedidoOutput;
                }

                var pedidoAgrupado = pedidosAgrupados[pedido.IdPedido];

                foreach (var combo in pedido.Combos)
                {
                    var comboOutput = new ComboOutput
                    {
                        IdCombo = combo.IdCombo,
                        Produto = new List<ProdutoOutput>()
                    };

                    foreach (var produtoCombo in combo.Produtos)
                    {
                        var produto = await _produtoContext.Produto.FindAsync(produtoCombo.IdProduto);
                        if (produto != null)
                        {
                            var produtoOutput = new ProdutoOutput
                            {
                                IdProduto = produto.IdProduto,
                                NomeProduto = produto.NomeProduto,
                                QuantidadeProduto = produtoCombo.Quantidade,
                                ValorProduto = produto.ValorProduto
                            };

                            comboOutput.Produto.Add(produtoOutput);
                        }
                    }

                    pedidoAgrupado.Combo.Add(comboOutput);
                }
            }

            return pedidosAgrupados.Values.ToList();
        }

        /// <summary>
        /// Registra um novo pedido no contexto do banco de dados e calcula o valor total do pedido com base nos produtos e quantidades.
        /// </summary>
        /// <param name="pedido">O pedido a ser registrado.</param>
        /// <returns>O pedido registrado, incluindo o cálculo do valor total.</returns>
        public async Task<Pedido> PostPedido(Pedido pedido)
        {
            // Cálculo do valor total
            pedido.ValorTotal = CalcularValorTotal(pedido);

            if (_context.Pedido is not null)
            {
                _context.Pedido.Add(pedido);
                await _context.SaveChangesAsync();
            }

            return pedido;
        }

        /// <summary>
        /// Obtém um pedido pelo seu id no banco de dados.
        /// </summary>
        /// <param name="idPedido">O id do pedido a ser obtido.</param>
        /// <returns>Um objeto Pedido com os dados do pedido encontrado ou null se não existir.</returns>
        /// <exception cref="DbException">Se ocorrer um erro ao acessar o banco de dados.</exception>
        public async Task<Pedido?> GetPedidoById(int idPedido)
        {
            // verifica se o DbSet Pedido não é nulo.
            if (_context.Pedido is not null)
                // retorna o primeiro pedido que corresponde ao idPedido ou null se não encontrar.
                return await _context.Pedido.FirstOrDefaultAsync(p => p.IdPedido == idPedido);
            // retorna null se o DbSet Pedido for nulo.
            return null;
        }

        /// <summary>
        /// Atualiza um pedido no banco de dados.
        /// </summary>
        /// <param name="pedido">O objeto Pedido com os dados atualizados.</param>
        /// <exception cref="DbUpdateException">Se ocorrer um erro ao atualizar o banco de dados.</exception>
        public async Task UpdatePedido(Pedido pedido)
        {
            // marca o estado do pedido como modificado.
            _context.Entry(pedido).State = EntityState.Modified;
            // salva as alterações no banco de dados de forma assíncrona.
            await _context.SaveChangesAsync();
        }

        #region [Métodos de verificação]
        /// <summary>
        /// Verifica se existe um cliente com o id especificado no banco de dados.
        /// </summary>
        /// <param name="clienteId">O id do cliente a ser verificado.</param>
        /// <returns>Um valor booleano que indica se o cliente existe ou não.</returns>
        /// <exception cref="DbException">Se ocorrer um erro ao acessar o banco de dados.</exception>
        public async Task<bool> ClienteExists(int clienteId)
        {
            if (_clienteContext.Cliente is not null)
                return await _clienteContext.Cliente.AnyAsync(c => c.IdCliente == clienteId);
            return false;
        }

        /// <summary>
        /// Verifica se existe um produto com o id especificado no banco de dados.
        /// </summary>
        /// <param name="produtoId">O id do produto a ser verificado.</param>
        /// <returns>Um valor booleano que indica se o produto existe ou não.</returns>
        /// <exception cref="DbException">Se ocorrer um erro ao acessar o banco de dados.</exception>
        public async Task<bool> ProdutoExists(int produtoId)
        {
            if (_produtoContext.Produto is not null)
                return await _produtoContext.Produto.AnyAsync(p => p.IdProduto == produtoId);
            return false;
        }
        #endregion

        #region [Métodos Privados]
        /// <summary>
        /// Calcula o valor total do pedido com base nos produtos e quantidades associados a ele.
        /// "O cálculo do valor total é uma operação relacionada à persistência de dados."
        /// </summary>
        /// <param name="pedido">O pedido para o qual o valor total deve ser calculado.</param>
        /// <returns>O valor total calculado para o pedido.</returns>
        private float CalcularValorTotal(Pedido pedido)
        {
            float valorTotal = 0;

            if (_produtoContext.Produto is not null)
            {
                foreach (var combo in pedido.Combos)
                {
                    foreach (var produtoCombo in combo.Produtos)
                    {
                        var produto = _produtoContext.Produto.FirstOrDefault(p => p.IdProduto == produtoCombo.IdProduto);

                        if (produto is not null)
                        {
                            valorTotal += produto.ValorProduto * produtoCombo.Quantidade;
                        }
                    }
                }
            }

            return valorTotal;
        }
        #endregion

    }
}
