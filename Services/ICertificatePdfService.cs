namespace Engineering_Hub.Services
{
    public interface ICertificatePdfService
    {
        byte[] Generate(Engineering_Hub.models.Certificate certificate);
    }
}