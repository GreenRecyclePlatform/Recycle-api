using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using recycle.Application.Interfaces;
using recycle.Application.Interfaces.IService.recycle.Application.Interfaces;
using recycle.Domain.Entities;
using System.Text;
using System.Text.Json;

namespace recycle.Application.Services
{
    /// <summary>
    /// خدمة الذكاء الاصطناعي لمشروع إعادة التدوير
    /// تستخدم Fireworks AI مع RAG
    /// </summary>
    public class RecycleRAGChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IKnowledgeService _knowledgeService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RecycleRAGChatService> _logger;

        public RecycleRAGChatService(
            HttpClient httpClient,
            IKnowledgeService knowledgeService,
            IConfiguration configuration,
            ILogger<RecycleRAGChatService> logger)
        {
            _httpClient = httpClient;
            _knowledgeService = knowledgeService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// معالجة سؤال المستخدم والحصول على رد من الـ AI
        /// </summary>
        public async Task<ChatResponseDto> GetChatResponse(string userMessage, Guid? userId = null)
        {
            try
            {
                // 1. البحث في قاعدة المعرفة
                var relevantInfo = await _knowledgeService.SearchRelevantInfo(userMessage);

                // 2. بناء الـ Context من المعلومات المسترجعة
                var context = BuildContext(relevantInfo);

                // 3. بناء الـ System Prompt
                var systemPrompt = BuildSystemPrompt(context);

                // 4. إرسال الطلب لـ Fireworks AI
                var aiResponse = await CallFireworksAPI(systemPrompt, userMessage);

                return new ChatResponseDto
                {
                    Message = aiResponse,
                    Timestamp = DateTime.UtcNow,
                    SourcesUsed = relevantInfo.Select(x => x.Category).Distinct().ToList(),
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetChatResponse");
                return new ChatResponseDto
                {
                    Message = "عذراً، حدث خطأ في معالجة طلبك. يرجى المحاولة مرة أخرى أو التواصل مع الدعم الفني.",
                    Timestamp = DateTime.UtcNow,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// بناء السياق من المعلومات المسترجعة
        /// </summary>
        private string BuildContext(List<KnowledgeItem> items)
        {
            if (!items.Any())
                return "لا توجد معلومات إضافية متاحة.";

            var sb = new StringBuilder();
            sb.AppendLine("=== معلومات من قاعدة البيانات ===\n");

            foreach (var item in items)
            {
                sb.AppendLine($"[{item.Category}]");
                sb.AppendLine(item.Content);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// بناء الـ System Prompt المخصص
        /// </summary>
        private string BuildSystemPrompt(string context)
        {
            return $@"أنت مساعد ذكي متخصص في منصة RecycleHub لإعادة تدوير المخلفات في مصر.

=== دورك ===
- مساعدة المستخدمين في فهم كيفية استخدام المنصة
- الإجابة على الأسئلة المتعلقة بالمواد المقبولة والأسعار
- شرح خطوات عمل طلب جمع
- توضيح طرق الدفع ومواعيد الاستلام
- مساعدة في تتبع الطلبات وحل المشاكل

=== المعلومات المتاحة ===
{context}

=== تعليمات مهمة ===
1. استخدم المعلومات المقدمة أعلاه فقط في إجاباتك
2. إذا لم تجد معلومة محددة، قل ذلك بوضوح ووجّه المستخدم للدعم الفني
3. كن ودوداً ومختصراً ومباشراً
4. استخدم الإيموجي بشكل مناسب لجعل الرد أكثر حيوية
5. إذا سُئلت عن معلومات شخصية (نقاط المستخدم، طلباته، إلخ)، اطلب من المستخدم تسجيل الدخول أو التواصل مع الدعم
6. لا تخترع معلومات - استخدم فقط ما هو متوفر في السياق

=== أسلوب الرد ===
- استخدم اللغة العربية الفصحى البسيطة
- قسّم الإجابات الطويلة لنقاط واضحة
- ضع الأرقام والأسعار بوضوح
- أضف روابط أو أرقام تواصل عند الحاجة";
        }

        /// <summary>
        /// إرسال الطلب لـ Fireworks API
        /// </summary>
        private async Task<string> CallFireworksAPI(string systemPrompt, string userMessage)
        {
            try
            {
                var apiKey = _configuration["Fireworks:ApiKey"];

                // ✅ Logging مفصل
                _logger.LogInformation("=== Fireworks API Call Started ===");
                _logger.LogInformation($"API Key configured: {!string.IsNullOrEmpty(apiKey)}");

                if (!string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogInformation($"API Key starts with 'fw_': {apiKey.StartsWith("fw_")}");
                    _logger.LogInformation($"API Key length: {apiKey.Length}");
                }

                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("Fireworks API Key is not configured");
                    throw new Exception("Fireworks API Key is not configured");
                }

                var endpoint = "https://api.fireworks.ai/inference/v1/chat/completions";
                var model = _configuration["Fireworks:Model"] ?? "accounts/fireworks/models/llama-v3p3-70b-instruct";

                _logger.LogInformation($"Endpoint: {endpoint}");
                _logger.LogInformation($"Model: {model}");

                var requestBody = new
                {
                    model = model,
                    max_tokens = 1000,
                    temperature = 0.7,
                    messages = new[]
                    {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            }
                };

                var jsonBody = JsonSerializer.Serialize(requestBody);
                _logger.LogInformation($"Request body length: {jsonBody.Length} chars");

                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                _logger.LogInformation("Sending request to Fireworks API...");

                var response = await _httpClient.PostAsync(endpoint, content);

                _logger.LogInformation($"Response Status Code: {(int)response.StatusCode} ({response.StatusCode})");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Fireworks API Error Response: {error}");
                    throw new Exception($"Fireworks API returned error: {response.StatusCode} - {error}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response content length: {responseContent.Length} chars");
                _logger.LogInformation($"Response content preview: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}...");

                var jsonResponse = JsonSerializer.Deserialize<FireworksResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (jsonResponse == null)
                {
                    _logger.LogError("Failed to deserialize response - jsonResponse is null");
                    _logger.LogError($"Raw response: {responseContent}");
                    return "لم أتمكن من الحصول على رد";
                }

                if (jsonResponse.Choices == null || jsonResponse.Choices.Count == 0)
                {
                    _logger.LogError("No choices in response");
                    _logger.LogError($"Raw response: {responseContent}");
                    return "لم أتمكن من الحصول على رد";
                }

                var messageContent = jsonResponse.Choices[0]?.Message?.Content;

                if (string.IsNullOrEmpty(messageContent))
                {
                    _logger.LogError("Message content is null or empty");
                    _logger.LogError($"Raw response: {responseContent}");
                    return "لم أتمكن من الحصول على رد";
                }

                _logger.LogInformation($"AI Response received: {messageContent.Substring(0, Math.Min(100, messageContent.Length))}...");
                _logger.LogInformation("=== Fireworks API Call Completed Successfully ===");

                return messageContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in CallFireworksAPI");
                throw;
            }
        }

        // ==================== DTOs ====================

        public class ChatResponseDto
        {
            public string Message { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
            public List<string> SourcesUsed { get; set; } = new();
            public bool Success { get; set; }
            public string? ErrorMessage { get; set; }
        }

        public class FireworksResponse
        {
            public string Id { get; set; } = string.Empty;
            public string Object { get; set; } = string.Empty;
            public long Created { get; set; }
            public string Model { get; set; } = string.Empty;
            public List<FireworksChoice>? Choices { get; set; }
            public FireworksUsage? Usage { get; set; }
        }

        public class FireworksChoice
        {
            public int Index { get; set; }
            public FireworksMessage? Message { get; set; }
            public string FinishReason { get; set; } = string.Empty;
        }

        public class FireworksMessage
        {
            public string Role { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
        }

        public class FireworksUsage
        {
            public int PromptTokens { get; set; }
            public int CompletionTokens { get; set; }
            public int TotalTokens { get; set; }
        }
    }
}