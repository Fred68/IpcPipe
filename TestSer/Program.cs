	

using Newtonsoft.Json;
using IpcMyData;
using System.Text;

Console.WriteLine("Hello, World!");

MyClass pippo = new MyClass(11.5,"TSt");

Console.WriteLine(pippo.ToString());

StringBuilder sb = new StringBuilder();

sb.AppendLine(JsonConvert.SerializeObject(pippo, typeof(MyClass), null));

MyClass? cl = null;
try
{
	object? x = JsonConvert.DeserializeObject(sb.ToString(),typeof(MyClass));
	cl = (MyClass?) x;
}
catch(Exception ex)
{
	sb.AppendLine(ex.ToString());
}

if(cl != null)
{
	sb.AppendLine(cl.ToString());
}

Console.WriteLine(sb.ToString());
Console.ReadKey();


