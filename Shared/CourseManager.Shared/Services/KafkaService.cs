using Confluent.Kafka;
using CourseManager.Shared.DTOs.Notification;
using System.Text.Json;

namespace CourseManager.Shared.Services
{
    /// <summary>
    /// Implementation của Kafka service
    /// </summary>
    public class KafkaService : IKafkaService, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly IConsumer<string, string> _consumer;
        private readonly ILogger<KafkaService> _logger;
        private readonly KafkaConfiguration _config;
        private readonly KafkaTopics _topics;
        private readonly List<CancellationTokenSource> _cancellationTokens = new();

        public KafkaService(
            KafkaConfiguration config,
            KafkaTopics topics,
            ILogger<KafkaService> logger)
        {
            _config = config;
            _topics = topics;
            _logger = logger;

            // Producer configuration
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _config.BootstrapServers,
                ClientId = _config.ClientId,
                Acks = Acks.All,
                Retries = _config.MaxRetryAttempts,
                RetryBackoffMs = _config.RetryBackoffMs,
                RequestTimeoutMs = _config.RequestTimeoutMs,
                SecurityProtocol = Enum.Parse<SecurityProtocol>(_config.SecurityProtocol, true)
            };

            _producer = new ProducerBuilder<string, string>(producerConfig).Build();

            // Consumer configuration
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _config.BootstrapServers,
                GroupId = _config.GroupId,
                ClientId = _config.ClientId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = _config.EnableAutoCommit,
                AutoCommitIntervalMs = _config.AutoCommitIntervalMs,
                SessionTimeoutMs = _config.SessionTimeoutMs,
                RequestTimeoutMs = _config.RequestTimeoutMs,
                SecurityProtocol = Enum.Parse<SecurityProtocol>(_config.SecurityProtocol, true)
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        }

        public async Task<bool> PublishAsync<T>(string topic, T message, string? key = null)
        {
            try
            {
                var json = JsonSerializer.Serialize(message);
                var kafkaMessage = new Message<string, string>
                {
                    Key = key ?? Guid.NewGuid().ToString(),
                    Value = json,
                    Timestamp = Timestamp.Default
                };

                var result = await _producer.ProduceAsync(topic, kafkaMessage);
                _logger.LogDebug("Published message to topic {Topic} with key {Key}", topic, key);
                return result.Status == PersistenceStatus.Persisted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message to topic {Topic}", topic);
                return false;
            }
        }

        public async Task<bool> PublishAsync(string topic, string message, string? key = null)
        {
            try
            {
                var kafkaMessage = new Message<string, string>
                {
                    Key = key ?? Guid.NewGuid().ToString(),
                    Value = message,
                    Timestamp = Timestamp.Default
                };

                var result = await _producer.ProduceAsync(topic, kafkaMessage);
                _logger.LogDebug("Published message to topic {Topic} with key {Key}", topic, key);
                return result.Status == PersistenceStatus.Persisted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message to topic {Topic}", topic);
                return false;
            }
        }

        public async Task<bool> PublishBatchAsync<T>(string topic, List<T> messages, string? key = null)
        {
            try
            {
                var tasks = messages.Select(message => PublishAsync(topic, message, key));
                var results = await Task.WhenAll(tasks);
                return results.All(r => r);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing batch messages to topic {Topic}", topic);
                return false;
            }
        }

        public async Task StartConsumingAsync<T>(string topic, Func<T, Task> messageHandler, CancellationToken cancellationToken = default)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _cancellationTokens.Add(cts);

            try
            {
                _consumer.Subscribe(topic);
                _logger.LogInformation("Started consuming from topic {Topic}", topic);

                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(cts.Token);
                        if (consumeResult?.Message?.Value != null)
                        {
                            var message = JsonSerializer.Deserialize<T>(consumeResult.Message.Value);
                            if (message != null)
                            {
                                await messageHandler(message);
                                _consumer.Commit(consumeResult);
                            }
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming message from topic {Topic}", topic);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Consuming from topic {Topic} was cancelled", topic);
            }
            finally
            {
                _consumer.Unsubscribe();
            }
        }

        public async Task StartConsumingAsync(string topic, Func<string, Task> messageHandler, CancellationToken cancellationToken = default)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _cancellationTokens.Add(cts);

            try
            {
                _consumer.Subscribe(topic);
                _logger.LogInformation("Started consuming from topic {Topic}", topic);

                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(cts.Token);
                        if (consumeResult?.Message?.Value != null)
                        {
                            await messageHandler(consumeResult.Message.Value);
                            _consumer.Commit(consumeResult);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming message from topic {Topic}", topic);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Consuming from topic {Topic} was cancelled", topic);
            }
            finally
            {
                _consumer.Unsubscribe();
            }
        }

        public async Task StopConsumingAsync()
        {
            foreach (var cts in _cancellationTokens)
            {
                cts.Cancel();
            }
            _cancellationTokens.Clear();
            await Task.CompletedTask;
        }

        public async Task<bool> CreateTopicAsync(string topic, int partitions = 1, short replicationFactor = 1)
        {
            // This would typically use AdminClient to create topics
            // For now, we'll just log and return true
            _logger.LogInformation("Topic {Topic} creation requested with {Partitions} partitions", topic, partitions);
            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteTopicAsync(string topic)
        {
            // This would typically use AdminClient to delete topics
            _logger.LogInformation("Topic {Topic} deletion requested", topic);
            return await Task.FromResult(true);
        }

        public async Task<List<string>> GetTopicsAsync()
        {
            // This would typically use AdminClient to list topics
            return await Task.FromResult(new List<string>());
        }

        // Event publishing methods
        public async Task PublishOrderEventAsync(OrderEventDto orderEvent)
        {
            await PublishAsync(_topics.OrderEvents, orderEvent, orderEvent.OrderId.ToString());
        }

        public async Task PublishPaymentEventAsync(PaymentEventDto paymentEvent)
        {
            await PublishAsync(_topics.PaymentEvents, paymentEvent, paymentEvent.OrderId.ToString());
        }

        public async Task PublishCourseEventAsync(CourseEventDto courseEvent)
        {
            await PublishAsync(_topics.CourseEvents, courseEvent, courseEvent.CourseId.ToString());
        }

        public async Task PublishUserEventAsync(UserEventDto userEvent)
        {
            await PublishAsync(_topics.UserEvents, userEvent, userEvent.UserId.ToString());
        }

        public async Task PublishSystemEventAsync(SystemEventDto systemEvent)
        {
            await PublishAsync(_topics.SystemEvents, systemEvent, systemEvent.EventId);
        }

        // Event consuming methods
        public async Task StartOrderEventConsumerAsync(Func<OrderEventDto, Task> handler, CancellationToken cancellationToken = default)
        {
            await StartConsumingAsync(_topics.OrderEvents, handler, cancellationToken);
        }

        public async Task StartPaymentEventConsumerAsync(Func<PaymentEventDto, Task> handler, CancellationToken cancellationToken = default)
        {
            await StartConsumingAsync(_topics.PaymentEvents, handler, cancellationToken);
        }

        public async Task StartCourseEventConsumerAsync(Func<CourseEventDto, Task> handler, CancellationToken cancellationToken = default)
        {
            await StartConsumingAsync(_topics.CourseEvents, handler, cancellationToken);
        }

        public async Task StartUserEventConsumerAsync(Func<UserEventDto, Task> handler, CancellationToken cancellationToken = default)
        {
            await StartConsumingAsync(_topics.UserEvents, handler, cancellationToken);
        }

        public async Task StartSystemEventConsumerAsync(Func<SystemEventDto, Task> handler, CancellationToken cancellationToken = default)
        {
            await StartConsumingAsync(_topics.SystemEvents, handler, cancellationToken);
        }

        public void Dispose()
        {
            _producer?.Dispose();
            _consumer?.Dispose();
            foreach (var cts in _cancellationTokens)
            {
                cts?.Dispose();
            }
        }
    }
}
