namespace recycle.Domain.Enums
{
    /// <summary>
    /// حالات طلب الجمع (للـ Chatbot)
    /// </summary>
    public static class RequestStatuses
    {
        public const string Pending = "Pending";           // جديد - في انتظار التوزيع
        public const string Assigned = "Assigned";         // تم التوزيع على سائق
        public const string InProgress = "InProgress";     // السائق في الطريق
        public const string PickedUp = "PickedUp";         // تم الجمع
        public const string Completed = "Completed";       // مكتمل
        public const string Cancelled = "Cancelled";       // ملغي
    }

    /// <summary>
    /// أنواع المستلمين للدفع (للـ Chatbot)
    /// </summary>
    public static class RecipientTypes
    {
        public const string User = "User";           // المستخدم العادي (صاحب الطلب)
        public const string Driver = "Driver";       // السائق
    }
}