using Microsoft.EntityFrameworkCore;
using recycle.Application.Interfaces; 
using recycle.Application.Interfaces.IService.recycle.Application.Interfaces;
using recycle.Domain.Entities;
using recycle.Domain.Enums; 
using recycle.Infrastructure;
using System;

namespace recycle.Infrastructure.Services
{
    
    public class RecycleKnowledgeService : IKnowledgeService
    {
        private readonly AppDbContext _context;
        private List<KnowledgeItem> _staticKnowledge;

        public RecycleKnowledgeService(AppDbContext context)
        {
            _context = context;
            InitializeStaticKnowledge();
        }

      //static info
        private void InitializeStaticKnowledge()
        {
            _staticKnowledge = new List<KnowledgeItem>
            {
                // ==================== معلومات المشروع ====================
                new KnowledgeItem
                {
                    Id = "project_info",
                    Category = "معلومات المشروع",
                    Content = @"منصة RecycleHub - منصة مصرية لإعادة تدوير المخلفات
                    
نحن منصة إلكترونية تربط بين المواطنين وخدمات جمع المواد القابلة لإعادة التدوير.

خدماتنا:
- جمع المواد من منزلك مجاناً
- دفع المال مقابل المواد المعاد تدويرها
- خدمة سريعة وموثوقة
- تطبيق سهل الاستخدام

المناطق المخدومة:
- القاهرة الكبرى (جميع الأحياء)
- الجيزة
- القليوبية
- الإسكندرية (قريباً)

مواعيد العمل: من السبت إلى الخميس، 9 صباحاً - 6 مساءً",
                    Keywords = new[] { "مشروع", "منصة", "خدمة", "معلومات", "عن المشروع", "ريسايكل" }
                },

                // ==================== المواد المقبولة ====================
                new KnowledgeItem
                {
                    Id = "accepted_materials",
                    Category = "المواد المقبولة",
                    Content = @"نقبل الأنواع التالية من المواد:

🔵 البلاستيك:
- زجاجات المياه (PET) - 3 جنيه/كيلو
- علب الشامبو والمنظفات (HDPE) - 2.5 جنيه/كيلو
- أكياس البلاستيك النظيفة - 2 جنيه/كيلو
⚠️ يجب أن تكون نظيفة وجافة

📄 الورق والكرتون:
- ورق أبيض (أوراق طابعة) - 2 جنيه/كيلو
- جرائد ومجلات - 1.5 جنيه/كيلو
- كرتون (كراتين التغليف) - 1.8 جنيه/كيلو
⚠️ لا نقبل الورق المبلل أو الملوث

🔩 المعادن:
- علب الألومنيوم (علب المشروبات) - 5 جنيه/كيلو
- معادن حديدية - 3 جنيه/كيلو
- نحاس وبرونز - 40 جنيه/كيلو

🪟 الزجاج:
- زجاجات وبرطمانات - 1 جنيه/كيلو
⚠️ يجب أن تكون سليمة (غير مكسورة)

📱 الإلكترونيات (حسب الحالة):
- موبايلات قديمة - من 50-200 جنيه
- أجهزة كمبيوتر - حسب الحالة
- كابلات وأسلاك - 15 جنيه/كيلو

❌ لا نقبل:
- بلاستيك ملوث أو متسخ
- ورق مبلل أو دهني
- زجاج مكسور بشكل خطر
- إلكترونيات تالفة تماماً",
                    Keywords = new[] { "مواد", "بلاستيك", "ورق", "كرتون", "معادن", "زجاج", "إلكترونيات", "أسعار", "سعر", "كيلو" }
                },

             
            };
        }

       //Searching 
        public async Task<List<KnowledgeItem>> SearchRelevantInfo(string query)
        {
            var results = new List<KnowledgeItem>();
            var queryLower = query.ToLower();

            // 1.search in staic info
            var staticResults = _staticKnowledge
                .Select(item => new
                {
                    Item = item,
                    Score = CalculateRelevanceScore(item, queryLower)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Take(3)
                .Select(x => x.Item)
                .ToList();

            results.AddRange(staticResults);

            // if QA about materialsInfo
            if (IsAboutMaterials(queryLower))
            {
                var materialsInfo = await GetMaterialsInfo();
                if (materialsInfo != null)
                    results.Add(materialsInfo);
            }

            return results;
        }

     
        private int CalculateRelevanceScore(KnowledgeItem item, string query)
        {
            int score = 0;
            
            //search in keyword
            foreach (var keyword in item.Keywords)
            {
                if (query.Contains(keyword.ToLower()))
                    score += 15;
            }

            // search in category
            if (item.Category.ToLower().Contains(query) || query.Contains(item.Category.ToLower()))
                score += 10;

            // search in QA
            var contentWords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in contentWords)
            {
                if (word.Length > 3 && item.Content.ToLower().Contains(word))
                    score += 2;
            }

            return score;
        }

      
        private bool IsAboutMaterials(string query)
        {
            string[] materialKeywords = { "مواد", "بلاستيك", "ورق", "كرتون", "معادن", "زجاج", "سعر", "كيلو", "أسعار" };
            return materialKeywords.Any(k => query.Contains(k));
        }

        //get it from DB
        private async Task<KnowledgeItem?> GetMaterialsInfo()
        {
            try
            {
                var materials = await _context.Materials
                    .Where(m => m.IsActive)
                    .Select(m => new
                    {
                        m.Name,
                        m.Description,
                        m.PricePerKg,
                        m.Unit
                    })
                    .ToListAsync();

                if (!materials.Any())
                    return null;

                var content = "المواد المتاحة حالياً وأسعارها:\n\n";
                foreach (var material in materials)
                {
                    content += $"• {material.Name}";
                    if (!string.IsNullOrEmpty(material.Description))
                        content += $" - {material.Description}";
                    content += $"\n  السعر: {material.PricePerKg} جنيه/{material.Unit ?? "كيلو"}\n\n";
                }

                return new KnowledgeItem
                {
                    Id = "db_materials",
                    Category = "المواد من قاعدة البيانات",
                    Content = content,
                    Keywords = new[] { "مواد", "أسعار", "سعر" }
                };
            }
            catch
            {
                return null;
            }
        }

     
        public List<KnowledgeItem> GetAllKnowledge()
        {
            return _staticKnowledge;
        }

    }
}