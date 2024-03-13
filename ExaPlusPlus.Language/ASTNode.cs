using System.Collections;
using System.Reflection;
using System.Text;

namespace ExaPlusPlus.Language;

public record ASTNode
{
    public string Accept(Compiler compiler)
    {
        return ((string)compiler.GetType()
            .GetMethod("Visit", BindingFlags.Public | BindingFlags.Instance, new[] { this.GetType() })!.Invoke(compiler, new[] { this })!)!;
    }
    public static string PrintNode(ASTNode node)
    {
        // print type name and string representation of all its fields
        var type = node.GetType();
        var fields = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var sb = new StringBuilder();
        sb.Append(type.Name);
        sb.Append("(");
        int i = 0;
        foreach (var field in fields)
        {
            if (i++ > 0) sb.Append(",");
            var value = field.GetValue(node);
            if (value is ASTNode valueNode)
            {
                sb.Append(PrintNode(valueNode));
            }
            else
            {
                sb.Append(value);
            }
            
            // if field is list - print all its elements
            if (field.PropertyType.IsGenericType && field.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var list = (IList)field.GetValue(node);
                sb.Append("[");
                int j = 0;
                sb.AppendLine();
                foreach (var item in list)
                {
                    if (j++ > 0)
                    {
                        sb.Append(",");
                        sb.AppendLine();
                    }
                    if (item is ASTNode itemNode)
                    {
                        sb.Append(PrintNode(itemNode));
                        
                    }
                    else
                    {
                        sb.Append(item);
                    }
                    
                }
                sb.Append("]");
            }
        }
        sb.Append(")");
        return sb.ToString();
        
        
    }
};