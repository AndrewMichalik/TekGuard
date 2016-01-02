using System;
using System.Collections;

namespace TGPAnalyzer
{
	internal class CategoryProbEntry : System.IComparable
	{
		private string		m_Key;
		private Int32		m_CatIndex;
		private double		m_Probability;
		private double		m_Interest;

		#region Constructors
		public CategoryProbEntry (string Key, Int32 CatIndex, double Probability, double Interest)
		{
			m_Key			= Key;
			m_CatIndex		= CatIndex;
			m_Probability	= Probability;
			m_Interest		= Interest;
			// m_Interest	= Math.Abs(.5 - Probability) * TotalCount;
		}
		#endregion

		public string Key
		{
			get {return(m_Key);}
		}
		public Int32 CatIndex
		{
			get {return(m_CatIndex);}
		}
		public double Probability
		{
			get {return(m_Probability);}
		}
		public double Interest
		{
			get {return(m_Interest);}
		}

		#region CompareTo
		public int CompareTo(object obj)
		{
			// Sort by normalized frequency, descending
			// return (((CategoryProbEntry)obj).Probability.CompareTo(Probability));
			// Sort by "interest level", descending
			return (((CategoryProbEntry)obj).Interest.CompareTo(Interest));
		}
		#endregion

		#region ToString()
		public override string ToString()
		{
			return (m_CatIndex + " " + m_Probability.ToString("0.00") +  ":" + m_Key);
		}
		#endregion

	}

}
