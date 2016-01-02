using System;

namespace TGPConnector
{
	/// <summary>
	/// Summary for MsgTokenEntry
	/// </summary>
	public class TokenEntry
	{
		private Int32[]		m_Frequency;
		private double[]	m_Probability;
		private DateTime	m_LastReceived;
		private Int32		m_TotalCount;

		#region Constructors

		public TokenEntry(int CategoryCount, DateTime LastReceived)
		{
			m_Frequency		= new Int32[CategoryCount];
			m_Probability	= new double[CategoryCount];
			m_LastReceived	= LastReceived;
			m_TotalCount	= 0;
		}

		public TokenEntry(TokenEntry EntryNew)
		{
			m_Frequency		= (Int32[]) EntryNew.m_Frequency.Clone();
			m_Probability	= (double[]) EntryNew.m_Probability.Clone();
			m_LastReceived	= EntryNew.m_LastReceived;
			m_TotalCount	= EntryNew.m_TotalCount;
		}
		#endregion

		#region TokenEntry[int index]
		public Int32 this[int index]
		{
			get
			{
				// Valid index?
				if ((index >= 0) && (index < CategoryCount))
				{
					// Index OK, return frequency bin count
					return (m_Frequency[index]);
				}
				else
				{
					// Bad index
					// m_Analyzer.FireLogException(this, "TokenEntry <" + index + ">", e.Message);
					return (0);
				}
			}
			set
			{
				// Valid index and frequency?
				if ((index >= 0) && (index < CategoryCount))
				{
					// Index OK, increment frequency bin count and total count
					m_Frequency[index] = value;
					m_TotalCount += value;
						
					// Count should never be less than zero
					if (value < 0)
					{
						// m_Analyzer.FireLogException(this, "TokenEntry <" + index + ">", e.Message);
						m_TotalCount = 0;
					}
				}
				else
				{
					// Bad index
					// m_Analyzer.FireLogException(this, "TokenEntry <" + index + ">", e.Message);
					return;
				}
			}
		}
		#endregion

		#region CalculateProbability
		public double[] CalculateProbability(Int64[] FrequencySum, out double[] Interest)
		{
			double[]	CountNormalized = new double[CategoryCount];
			Int64		FrequencyTotal = 0;

			// Calculate normalized counts by dividing by category total token count
			for (int ii=0; ii<CategoryCount; ii++)
			{
				CountNormalized[ii] = this[ii] / (double) FrequencySum[ii];
				FrequencyTotal += FrequencySum[ii];
			}

			// Calculate the denonimator for this category
			double Denonimator = 0;
			for (int ii=0; ii<CategoryCount; ii++)
			{
				Denonimator += CountNormalized[ii];
			}

			// Calculate the probability for each category
			for (int ii=0; ii<CategoryCount; ii++)
			{
				m_Probability[ii] = CountNormalized[ii] / Denonimator;
			}

			// Calculate the interest RMS value scalar for this token
			Interest = new double[CategoryCount];
			for (int ii=0; ii<CategoryCount; ii++)
			{
				double VectorP = m_Probability[ii] * m_Probability[ii];
				double VectorD = (2 * Math.Abs(.5 - m_Probability[ii]));
				double VectorC = m_TotalCount / (double) FrequencyTotal;
					
				// Fewer false positives:
				// Interest[ii] = m_Probability[ii] * ((VectorD * VectorD * VectorD) + Math.Sqrt(VectorC));
				Interest[ii] = m_Probability[ii] * ((VectorD * VectorD * VectorD * VectorD) + Math.Sqrt(VectorC));

				// Appears to catch the most spam:
				// Interest[ii] = m_Probability[ii] * ((VectorD * VectorD) + Math.Sqrt(VectorC));
					
				// This is a pretty good one:
				// Interest[ii] = m_Probability[ii] * (1 + m_TotalCount/ (double) FrequencyTotal);
					
				// Interesting:
				// Interest[ii] = m_Probability[ii] * (VectorD + m_TotalCount/ (double) FrequencyTotal);
					
				//Interest[ii] = m_Probability[ii];
				//Interest[ii] = m_Probability[ii] * (Math.Sqrt(VectorD) + Math.Sqrt(VectorC));
				//Interest[ii] = m_Probability[ii] * Math.Sqrt((VectorD * VectorD) + (VectorC * VectorC));
				//Interest[ii] = m_Probability[ii] * Math.Sqrt((VectorD * VectorD) + (VectorD * VectorD));
				//Interest[ii] = m_Probability[ii] * ((2 * Math.Abs(.5 - m_Probability[ii])) * m_TotalCount/ (double) FrequencyTotal);
				//Interest[ii] = m_Probability[ii] * (1 + (2 * Math.Abs(.5 - m_Probability[ii])) * m_TotalCount/ (double) FrequencyTotal);
				//Interest[ii] = m_Probability[ii] * (1 + CountNormalized[ii]);
				//Interest[ii] = m_Probability[ii] * (1 + Math.Sqrt(m_TotalCount/ (double) FrequencyTotal));
				//Interest[ii] = m_Probability[ii] * (2 * Math.Abs(.5 - m_Probability[ii])) * (m_TotalCount/ (double) FrequencyTotal);
				//Interest[ii] = Denonimator;
				//Interest[ii] = m_Probability[ii] * (1 + (VectorD * VectorD * Math.Sqrt (m_TotalCount / (double) FrequencyTotal)));
				//Interest[ii] = m_Probability[ii] * (VectorD * VectorD * Math.Sqrt (m_TotalCount / (double) FrequencyTotal));
				//Interest[ii] = m_Probability[ii] * (1 + (VectorD * VectorD * Math.Sqrt (CountNormalized[ii])));
				//Interest[ii] = Math.Sqrt (VectorP + (VectorD * VectorD) + VectorC);
				//Interest[ii] = m_Probability[ii] * (1 + Math.Sqrt ((VectorD * VectorD) + VectorC));
				//Interest[ii] = m_Probability[ii];
			}

			// Return the category probability list
			return (m_Probability);
		}
		#endregion

		#region LastReceived
		public DateTime LastReceived
		{
			get {return(m_LastReceived);}
			// Set most recent date
			set {if (value > m_LastReceived) m_LastReceived = value;}
		}
		#endregion

		#region TotalCount
		private Int64 TotalCount
		{
			get {return(m_TotalCount);}
		}
		#endregion

		#region CategoryCount
		private int CategoryCount
		{
			get {return(m_Frequency.Length);}
		}
		#endregion

	}
}
