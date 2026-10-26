namespace JobPortal.Domain.Enums;

public enum ApplicationStatus
{
    Pending = 0,     // надіслано, роботодавець ще не дивився
    Viewed = 1,      // переглянуто
    Invited = 2,     // запросили на співбесіду
    Rejected = 3     // відмова
}