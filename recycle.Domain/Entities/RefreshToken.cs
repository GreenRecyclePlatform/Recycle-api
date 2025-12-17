using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string JwtTokenId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Refresh_Token { get; set; }
        public bool IsValid { get; set; }
    }
}
