using static DapperCodeGenerator.Services.TypeMapper;
using static DapperCodeGenerator.Utils.Util;

namespace DapperCodeGenerator.Models
{
    public class Column(string fieldName)
    {
        public string Name { get;  } = GetColumnName(fieldName);
        public string DataType { get;  } =  GetDataType(fieldName);
        public string? DotNetType { get;  } = GetDotNetType(fieldName);
        public string? DbType { get;  } = GetDbType(fieldName);
        public int Length { get;  } = GetColumnLength(fieldName);
        public bool IsRequired { get;  } = fieldName[^4..] == " NOT";
    }
}

