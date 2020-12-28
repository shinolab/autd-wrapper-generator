namespace autd_wrapper_generator.lib
{
    internal interface ICodeGenerator
    {
        string GetCommentPrefix();
        string GetFileHeader();
        string GetFileFooter();
        string GetFunctionDefinition(Function func);
    }
}
