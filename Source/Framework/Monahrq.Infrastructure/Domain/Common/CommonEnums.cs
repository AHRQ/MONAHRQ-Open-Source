using System;
using System.ComponentModel;

namespace Monahrq.Infrastructure.Domain.Common
{
    [Serializable, Flags]
    public enum GenderEnum
    {
        Male = 0,
        Female = 1
    }

    [Serializable, Flags]
    public enum LanguageModeEnum
    {
        [Description("English")]
        English = 0,
        [Description("Spanish")]
        Spanish = 1,
        [Description("Vietnamese")]
        Vietnamese = 2,
        [Description("Chinese")]
        Chinese = 3,
        [Description("Portuguese")]
        Portuguese = 4,
        [Description("Dutch")]
        Dutch = 5,
        [Description("French")]
        French = 5,
        [Description("Farci")]
        Farci = 6,
        [Description("Bangalie")]
        Bangalie = 7,
        [Description("Russian")]
        Russian = 8
    }


    [Serializable, Flags]
    public enum CredentialEnum
    {
        [Description("Doctor of Medicine")]
        MD = 0,
        [Description("DO")]
        DO = 1,
        [Description("DPM")]
        DPM = 2
    }


    [Serializable, Flags]
    public enum TopicTypeEnum
    {
        [Description("Hospital")]
        Hospital,

        [Description("Nursing Home")]
        NursingHome,

        [Description("Physician")]
        Physician
    }

    [Serializable, Flags]
    public enum TopicCategoryTypeEnum
    {

        [Description("Condition")]
        Condition,

        [Description("Topic")]
        Topic
    }
}