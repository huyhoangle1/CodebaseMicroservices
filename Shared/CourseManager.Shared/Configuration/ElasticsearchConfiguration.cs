namespace CourseManager.Shared.Configuration
{
    /// <summary>
    /// Cấu hình Elasticsearch
    /// </summary>
    public class ElasticsearchConfiguration
    {
        public string BaseUrl { get; set; } = "http://localhost:9200";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string CertificateFingerprint { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = false;
        public bool VerifySsl { get; set; } = true;
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxRetries { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 1;
        public int NumberOfShards { get; set; } = 1;
        public int NumberOfReplicas { get; set; } = 1;
        public int MaxBulkSize { get; set; } = 1000;
        public int MaxBulkBytes { get; set; } = 10485760; // 10MB
        public bool EnableCompression { get; set; } = true;
        public bool EnableSniffing { get; set; } = false;
        public List<string> NodeUrls { get; set; } = new();
        public Dictionary<string, object> DefaultSettings { get; set; } = new();
        public Dictionary<string, object> IndexTemplates { get; set; } = new();
        public bool EnableLogging { get; set; } = true;
        public string LogLevel { get; set; } = "Information";
    }

    /// <summary>
    /// Cấu hình Index Template
    /// </summary>
    public class IndexTemplateConfiguration
    {
        public string Name { get; set; } = string.Empty;
        public List<string> IndexPatterns { get; set; } = new();
        public int Priority { get; set; } = 0;
        public Dictionary<string, object> Settings { get; set; } = new();
        public Dictionary<string, object> Mappings { get; set; } = new();
        public Dictionary<string, object> Aliases { get; set; } = new();
    }

    /// <summary>
    /// Cấu hình Analyzer
    /// </summary>
    public class AnalyzerConfiguration
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "custom";
        public string Tokenizer { get; set; } = "standard";
        public List<string> CharFilters { get; set; } = new();
        public List<string> TokenFilters { get; set; } = new();
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    /// <summary>
    /// Cấu hình Index Lifecycle Management
    /// </summary>
    public class IndexLifecycleConfiguration
    {
        public string PolicyName { get; set; } = string.Empty;
        public List<LifecyclePhase> Phases { get; set; } = new();
        public bool Enabled { get; set; } = true;
    }

    /// <summary>
    /// Lifecycle Phase
    /// </summary>
    public class LifecyclePhase
    {
        public string Name { get; set; } = string.Empty;
        public string MinAge { get; set; } = string.Empty;
        public Dictionary<string, object> Actions { get; set; } = new();
    }

    /// <summary>
    /// Cấu hình Security
    /// </summary>
    public class ElasticsearchSecurityConfiguration
    {
        public bool EnableSecurity { get; set; } = false;
        public string ApiKey { get; set; } = string.Empty;
        public string BearerToken { get; set; } = string.Empty;
        public string ClientCertificatePath { get; set; } = string.Empty;
        public string ClientCertificatePassword { get; set; } = string.Empty;
        public string ServerCertificatePath { get; set; } = string.Empty;
        public bool VerifyServerCertificate { get; set; } = true;
        public List<string> AllowedHosts { get; set; } = new();
        public List<string> AllowedOrigins { get; set; } = new();
    }

    /// <summary>
    /// Cấu hình Monitoring
    /// </summary>
    public class ElasticsearchMonitoringConfiguration
    {
        public bool EnableHealthChecks { get; set; } = true;
        public int HealthCheckIntervalSeconds { get; set; } = 30;
        public bool EnableMetrics { get; set; } = true;
        public int MetricsIntervalSeconds { get; set; } = 60;
        public bool EnableLogging { get; set; } = true;
        public string LogLevel { get; set; } = "Information";
        public bool EnablePerformanceCounters { get; set; } = false;
        public List<string> MonitoredIndices { get; set; } = new();
    }

    /// <summary>
    /// Cấu hình Backup
    /// </summary>
    public class ElasticsearchBackupConfiguration
    {
        public bool EnableBackup { get; set; } = false;
        public string BackupRepository { get; set; } = string.Empty;
        public string BackupLocation { get; set; } = string.Empty;
        public string BackupSchedule { get; set; } = "0 2 * * *"; // Daily at 2 AM
        public int RetentionDays { get; set; } = 30;
        public List<string> BackupIndices { get; set; } = new();
        public bool CompressBackups { get; set; } = true;
        public string CompressionLevel { get; set; } = "6";
    }

    /// <summary>
    /// Cấu hình Search
    /// </summary>
    public class ElasticsearchSearchConfiguration
    {
        public int DefaultPageSize { get; set; } = 10;
        public int MaxPageSize { get; set; } = 100;
        public int DefaultTimeoutSeconds { get; set; } = 30;
        public bool EnableHighlighting { get; set; } = true;
        public int HighlightFragmentSize { get; set; } = 150;
        public int HighlightNumberOfFragments { get; set; } = 3;
        public bool EnableSuggestions { get; set; } = true;
        public int SuggestionSize { get; set; } = 5;
        public bool EnableFuzzySearch { get; set; } = true;
        public int FuzzyMaxEdits { get; set; } = 2;
        public bool EnableAutoComplete { get; set; } = true;
        public int AutoCompleteSize { get; set; } = 10;
    }

    /// <summary>
    /// Cấu hình Aggregations
    /// </summary>
    public class ElasticsearchAggregationConfiguration
    {
        public bool EnableAggregations { get; set; } = true;
        public int MaxAggregationBuckets { get; set; } = 10000;
        public int DefaultAggregationSize { get; set; } = 100;
        public bool EnableNestedAggregations { get; set; } = true;
        public int MaxNestedDepth { get; set; } = 5;
        public bool EnablePipelineAggregations { get; set; } = true;
        public bool EnableScriptedAggregations { get; set; } = false;
    }

    /// <summary>
    /// Cấu hình Index Patterns
    /// </summary>
    public class IndexPatternConfiguration
    {
        public string Name { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public string TimeField { get; set; } = string.Empty;
        public List<string> Fields { get; set; } = new();
        public Dictionary<string, object> Settings { get; set; } = new();
        public bool IsDefault { get; set; } = false;
    }
}
