using System;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using ProcessadorAcoes.Models;

namespace ProcessadorAcoes.Data
{
    public class AcoesRepository
    {
        private readonly ConnectionMultiplexer _conexaoRedis;
        private readonly string _prefixoChave;

        public AcoesRepository(IConfiguration configuration)
        {
            _conexaoRedis = ConnectionMultiplexer.Connect(
                configuration.GetConnectionString("BaseCotacoes"));
            _prefixoChave = configuration["PrefixoChaveRedis"];
        }

        public void Save(Acao acao)
        {
            acao.UltimaAtualizacao = DateTime.Now;
            _conexaoRedis.GetDatabase().StringSet(
                _prefixoChave + acao.Codigo,
                JsonSerializer.Serialize(acao),
                expiry: null);
        }
    }
}