using System;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Confluent.Kafka;
using Serilog.Core;
using ProcessadorAcoes.Models;
using ProcessadorAcoes.Validators;
using ProcessadorAcoes.Data;

namespace ProcessadorAcoes
{
    public class ConsoleApp
    {
        private readonly Logger _logger;
        private readonly IConfiguration _configuration;
        private readonly AcoesRepository _repository;

        public ConsoleApp(Logger logger, IConfiguration configuration,
            AcoesRepository repository)
        {
            _logger = logger;
            _configuration = configuration;
            _repository = repository;
        }

        public void Run()
        {
           _logger.Information("Testando o consumo de mensagens com Kafka");

            string nomeTopic = _configuration["Kafka_Topic"];
            _logger.Information($"Topic = {nomeTopic}");

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                using (var consumer = GetConsumerBuilder())
                {
                    consumer.Subscribe(nomeTopic);

                    try
                    {
                        while (true)
                        {
                            var cr = consumer.Consume(cts.Token);
                            string dados = cr.Message.Value;

                            _logger.Information(
                                $"Mensagem lida: {dados}");

                            var acao = JsonSerializer.Deserialize<Acao>(dados,
                                new JsonSerializerOptions()
                                {
                                    PropertyNameCaseInsensitive = true
                                });

                            var validationResult = new AcaoValidator().Validate(acao);
                            if (validationResult.IsValid)
                            {
                                _repository.Save(acao);
                                _logger.Information("Ação registrada com sucesso!");
                            }
                            else
                                _logger.Error("Dados inválidos para a Ação");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        consumer.Close();
                        _logger.Warning("Cancelada a execução do Consumer...");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Exceção: {ex.GetType().FullName} | " +
                             $"Mensagem: {ex.Message}");
            }
        }

        private IConsumer<Ignore, string> GetConsumerBuilder()
        {
            string bootstrapServers = _configuration["Kafka_Broker"];
            _logger.Information($"BootstrapServers = {bootstrapServers}");

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = $"consoleapp-di-redis",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _logger.Information($"GroupId = {config.GroupId}");

            return new ConsumerBuilder<Ignore, string>(config).Build();
        }
    }
}