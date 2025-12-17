using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recycle.Application.Interfaces;
using recycle.Application.Interfaces.IService.recycle.Application.Interfaces;
using recycle.Application.Services;
using recycle.Domain.Entities;
using System.Security.Claims;

namespace recycle.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly RecycleRAGChatService _chatService;
        private readonly IKnowledgeService _knowledgeService; 
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(
            RecycleRAGChatService chatService,
            IKnowledgeService knowledgeService,
            ILogger<ChatbotController> logger)
        {
            _chatService = chatService;
            _knowledgeService = knowledgeService;
            _logger = logger;
        }

        [HttpPost("chat")]
       // [ProducesResponseType(typeof(ChatResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { error = "الرسالة لا يمكن أن تكون فارغة" });
                }

                if (request.Message.Length > 500)
                {
                    return BadRequest(new { error = "الرسالة طويلة جداً. الحد الأقصى 500 حرف" });
                }

                Guid? userId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (Guid.TryParse(userIdClaim, out var parsedUserId))
                    {
                        userId = parsedUserId;
                    }
                }

                var response = await _chatService.GetChatResponse(request.Message, userId);

                if (!response.Success)
                {
                    _logger.LogError($"Chat service failed: {response.ErrorMessage}");
                    return StatusCode(500, new
                    {
                        error = "عذراً، حدث خطأ في معالجة طلبك",
                        message = response.Message
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Chat endpoint");
                return StatusCode(500, new
                {
                    error = "حدث خطأ غير متوقع",
                    message = "يرجى المحاولة مرة أخرى أو التواصل مع الدعم الفني"
                });
            }
        }

        /// <summary>
        /// الحصول على جميع المعلومات المتاحة في قاعدة المعرفة
        /// </summary>
        [HttpGet("knowledge")]
        [ProducesResponseType(typeof(List<KnowledgeItem>), 200)]
        public IActionResult GetKnowledge()
        {
            try
            {
                var knowledge = _knowledgeService.GetAllKnowledge();
                return Ok(new
                {
                    count = knowledge.Count,
                    items = knowledge.Select(k => new
                    {
                        k.Id,
                        k.Category,
                        contentPreview = k.Content.Length > 100
                            ? k.Content.Substring(0, 100) + "..."
                            : k.Content,
                        k.Keywords
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetKnowledge endpoint");
                return StatusCode(500, new { error = "حدث خطأ في جلب المعلومات" });
            }
        }

        /// <summary>
        /// أسئلة سريعة جاهزة للمستخدمين
        /// </summary>
        [HttpGet("quick-questions")]
        [ProducesResponseType(typeof(List<QuickQuestionDto>), 200)]
        public IActionResult GetQuickQuestions()
        {
            var questions = new List<QuickQuestionDto>
            {
                new QuickQuestionDto
                {
                    Id = "q1",
                    Question = "ازاي أعمل طلب جمع؟",
                    Icon = "📦",
                    Category = "الطلبات"
                },
                new QuickQuestionDto
                {
                    Id = "q2",
                    Question = "إيه المواد اللي بتقبلوها؟",
                    Icon = "♻️",
                    Category = "المواد"
                },
                new QuickQuestionDto
                {
                    Id = "q3",
                    Question = "كام سعر الكيلو لكل مادة؟",
                    Icon = "💰",
                    Category = "الأسعار"
                },
                new QuickQuestionDto
                {
                    Id = "q4",
                    Question = "متى يصل السائق؟",
                    Icon = "🚗",
                    Category = "التوصيل"
                },
                new QuickQuestionDto
                {
                    Id = "q5",
                    Question = "ازاي أستلم فلوسي؟",
                    Icon = "💳",
                    Category = "الدفع"
                },
                new QuickQuestionDto
                {
                    Id = "q6",
                    Question = "ازاي أتابع طلبي؟",
                    Icon = "📍",
                    Category = "التتبع"
                }
            };

            return Ok(questions);
        }

        /// <summary>
        /// فحص صحة الخدمة
        /// </summary>
        [HttpGet("health")]
        [ProducesResponseType(200)]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "healthy",
                service = "RecycleHub Chatbot",
                timestamp = DateTime.UtcNow
            });
        }
    }

    // ==================== DTOs ====================

    public class ChatRequestDto
    {
        public string Message { get; set; } = string.Empty;
        public string? SessionId { get; set; }
    }

    public class QuickQuestionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}