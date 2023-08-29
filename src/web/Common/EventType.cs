using System.Text.Json.Serialization;

namespace FfAdmin.Common
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EventType
    {
        NONE,
        DONA_NEW,
        DONA_UPDATE_CHARITY,
        META_NEW_OPTION,
        META_NEW_CHARITY,
        META_UPDATE_FRACTIONS,
        CONV_LIQUIDATE,
        CONV_EXIT,
        CONV_TRANSFER,
        CONV_ENTER,
        CONV_INVEST,
        AUDIT,
        DONA_CANCEL,
        META_UPDATE_CHARITY,
        CONV_INCREASE_CASH,
        META_CHARITY_PARTITION
    }
}
