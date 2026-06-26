namespace Application.CloudServices.Dtos
{
    public class UserStorageDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public long UsedBytes { get; set; }
        public long MaxBytes { get; set; }
    }
}
