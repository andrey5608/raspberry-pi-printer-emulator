using System.Text.Json.Serialization;

namespace EscPosDecoderApi.Models
{
    public class ReceiptCreationResponse
    {
        [JsonPropertyName("text")]
        public string Text;

        [JsonPropertyName("sentToDiditApi")]
        public bool SentToDiditApi;

        public ReceiptCreationResponse(string text, bool sentToDiditApi)
        {
            Text = text;
            SentToDiditApi = sentToDiditApi;
        }
    }
}
