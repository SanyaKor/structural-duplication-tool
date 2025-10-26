abstract class test1
{
    
	// duplicated parameter added
    public abstract void test1_method1(string param1, string param1_duplicate);
    
	// duplicated parameter added
    public abstract void test1_method2(string param1, string param1_duplicate);
    
	// duplicated parameter added
    public abstract void test1_method3(string? param1, string? param1_duplicate);
    
	// duplicated parameter added
    public abstract void test1_method3(ref string? param1, ref string? param1_duplicate);
    public abstract void test1_method3(string param1, string param2, string param3);
    
	// duplicated parameter added
    public abstract string test1_method4(string param1, string param1_duplicate);
}