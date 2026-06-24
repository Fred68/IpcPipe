using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IpcMyData
{
	public class MyClass
	{
		double _x;
		string _str;

		public double X
		{
			get { return _x;}
			set { _x = value; }
		}	

		public string Str
		{
			get{ return _str;}
			set { _str = value; }
		}
		public MyClass()
		{}

		public MyClass(double x, string str)
		{
			_x = x;
			_str = str;
		}
	}

}
