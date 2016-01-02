using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Text;
using TGPConnector;

namespace TGPAnalyzer
{
	public class TokenList
	{
		private		Analyzer			m_Analyzer;
		private		MsgTokens			m_MsgTokens;
		private		Regex				m_regexSplit;
		
// AJM To-Do: Future release, clean up this local variables.
// Possibly separate into count and check classes
// Yikes! Need to lock the cache. One thread may be rebuilding the Analyzer,
// and another may be analyzing an incoming message using m_CountCache.
// Exception: TGPAnalyzer.TokenList.DoCountCommit	Collection was modified; enumeration operation may not execute.	CatIndex=<1>
private	TokenCache	m_CheckCache;
private	TokenCache	m_CountCache;

		// Token table constants
		private const string		COL_TOKEN			= "Token";
		private const string		COL_CATEGORY		= "C_";
		private const string		COL_DATELAST		= "DateLast";

		// Message Head / Tail size constants
		private const	float		PERCENT_THRESH		= 10000;	// Maximum text length
		private const	float		PERCENT_HEAD		= 20;		// Head size if over max
		private const	float		PERCENT_TAIL		= 20;		// Tail size if over max

		// Class constants		
		private const bool			CASE_SENSITIVE		= false;
		private	const Int32			ONE_ENTRY			= 1;

		#region Constructors / Destructors
		public TokenList(Analyzer Analyzer, MsgTokens MsgTokens, string regexString)
		{
			// Save pointer to parent procedure
			m_Analyzer = Analyzer;
			
			// Initialize MsgTokens
			m_MsgTokens = MsgTokens;

			try
			{
				m_regexSplit = new Regex(regexString);
			}
			catch(Exception ex)
			{
				m_Analyzer.FireLogException(ex, "regexString", regexString);
			}
		}
		#endregion

		#region Initialize
		internal bool Initialize ()
		{
			return (m_MsgTokens.Initialize());
		}
		#endregion

		#region Ready
		internal double Ready ()
		{
			// Determine if database is ready for analysis
			if (m_MsgTokens == null) return (0);
			return (m_MsgTokens.Ready());
		}
		#endregion

		#region Reset
		internal void Reset ()
		{
			// Clear the contents of the Token Table
			m_MsgTokens.Reset ();
		}
		#endregion

		#region DoCountBegin
		internal void DoCountBegin(Int32 CategoryCount)
		{
			// Create the memory resident token table
			m_CountCache = new TokenCache(CategoryCount);
		}
		#endregion

		#region DoCount
		internal void DoCount(Int32 CategoryID, int CategoryIndex, string Text, DateTime LastReceived)
		{
			string Token;

			// Trim blanks; Truncate message if too long
			Text = Text.Trim();
			if (Text.Length > PERCENT_THRESH)
			{
			}

			// Drop upper/lower case information
			if (!CASE_SENSITIVE) Text = Text.ToLower();

			// Calculate the Category row name
			string	ColCategory = COL_CATEGORY + CategoryID.ToString();

			// Count each word
			foreach (string word in m_regexSplit.Split(Text))
			{
				if (!IsGoodToken(word, out Token)) continue;

				// Add or update this word to the Token List
				try
				{
					// UpdateToken (m_dtMsgTokens, Token, ColCategory, LastReceived, ONE_ENTRY);
					TokenEntry TokenUpdate = new TokenEntry(m_CountCache.CategoryCount, LastReceived);
					TokenUpdate[CategoryIndex] = ONE_ENTRY;
					m_CountCache.Update (Token, TokenUpdate);
				}
				catch(Exception ex)
				{
					m_Analyzer.FireLogException(ex, "word", word);
				}
			}

		}
		#endregion

		#region DoCountCommit
		internal bool DoCountCommit (Int32 CategoryID, int CategoryIndex)
		{
			// Generate the Token table name and Category column names
			string ColCategory = COL_CATEGORY + CategoryID.ToString();

			try
			{
				// Write the cached Tokens to the datatable
				foreach (DictionaryEntry item in m_CountCache)
				{
					m_MsgTokens.UpdateToken(item.Key.ToString(), ColCategory, ((TokenEntry) item.Value).LastReceived, ((TokenEntry) item.Value)[CategoryIndex]);
				}

				// Succcess
				return true;
			}
			catch(Exception ex)
			{
				m_Analyzer.FireLogException(ex, "CatIndex", CategoryIndex.ToString());
				return false;
			}
		}
		#endregion

		#region DoCountPersist
		internal bool DoCountPersist (Int32 DayKeepCount)
		{
			// Persist if the Token Table not match the persisted binary version, otherwise return success
			return (m_MsgTokens.PersistIsDirty ? m_MsgTokens.Persist(DayKeepCount) : true);
		}
		#endregion

		#region DoCountBackup
		internal bool DoCountBackup ()
		{
			// Backup if the persisted binary version does not match the XML backup, otherwise return success
			return (m_MsgTokens.BackupIsDirty ? m_MsgTokens.Backup() : true);
		}
		#endregion

		#region DoCheckBegin
		internal ArrayList DoCheckBegin(CategoryList AnalyzerCategories, string[] aText)
		{
			ArrayList	CategoryProbList = new ArrayList();

			// Calculate the Category row names
			string[] ColCategory = new string[AnalyzerCategories.Count];
			for (int ii=0; ii<AnalyzerCategories.Count; ii++)
			{
				ColCategory[ii] = COL_CATEGORY + AnalyzerCategories[ii].CategoryID.ToString();
			}

			// Create a new table for this set of tokens
			m_CheckCache = new TokenCache(AnalyzerCategories.Count);

			// Count each word in each string
			if (aText != null) foreach (string Text in aText)
			{
				CategoryProbList = DoCheckBeginCache(CategoryProbList, ColCategory, Text);
			}

			// Return the list of active tokens
			return (CategoryProbList);
		}
		
		private ArrayList DoCheckBeginCache(ArrayList CategoryProbList, string[] ColCategory, string Text)
		{
			// Trim blanks; Truncate message if too long
			Text = Text.Trim();
			if (Text.Length > PERCENT_THRESH)
			{
			}

			// Drop upper/lower case information
			if (!CASE_SENSITIVE) Text = Text.ToLower();

			// Count each word
			foreach (string word in m_regexSplit.Split(Text))
			{
				string Token;
				if (!IsGoodToken(word, out Token)) continue;

				// Add or update this word to the Addendum based on the Token List
				try
				{
					// Look for token in the full list of categorized tokens
					// If not found, ignore token - no information available
					TokenEntry TokenNew = m_MsgTokens.FindToken(ColCategory, Token);
					if (TokenNew != null)
					{
						// Interesting enough?
						// const Int32	INTERESTLEVEL = 3;
						// if (!(TokenEntry.TotalCount > INTERESTLEVEL)) continue;

						// Add or update this within the subset list
						m_CheckCache.Update(Token, new TokenEntry(TokenNew));

						// Add one occurrence only to the subset list
						// if (TokenListSubset[Token] == null) TokenListSubset.Add(Token, TokenEntry);
					}
				}
				catch(Exception ex)
				{
					m_Analyzer.FireLogException(ex, "word", word);
				}
			}

			// Calculate probabilities for token subset
			foreach (DictionaryEntry item in m_CheckCache)
			{
				try
				{
					double[] Interest;
					TokenEntry TokenEntry = ((TokenEntry) item.Value);
					double[] Probability = TokenEntry.CalculateProbability(m_CheckCache.FrequencySum, out Interest);

					// Unroll the probabilities and categories so they can be sorted
					for (int ii=0; ii<Probability.Length; ii++)
					{
						CategoryProbList.Add (new CategoryProbEntry(item.Key.ToString(), ii, Probability[ii], Interest[ii]));
					}
				}
				catch(Exception ex)
				{
					m_Analyzer.FireLogException(ex, "Key", item.Key.ToString());
				}
			}

			// Return the list of active tokens
			return (CategoryProbList);
		}

		#endregion

		#region DoCheck
		internal double[] DoCheck(ArrayList CategoryProbList, int CategoryCount)
		{
			int CatCount = CategoryCount;

			//AJM ToDo: CatCount must be > 1
			if (CatCount <= 1) return (new double[] {1.0});

			// Sort the probabilities and categories by descending probability
			CategoryProbList.Sort();

			// Calculate the probabilities using Bayes Theorem
			// ***********************************************************
			//						(r1)(r2)...(rN)
			// Pr{TRUE} = ------------------------------------------
			//				(r1)(r2)...(rN) + (1-r1)(1-r2)...(1-rN)
			// num = (probs(1) * probs(2) * probs(3) * probs(4) * probs(5) * probs(6) * probs(7) * probs(8) * probs(9) * probs(10) * probs(11) * probs(12) * probs(13) * probs(14) * probs(15))
			// den = num + (1 - probs(1)) * (1 - probs(2)) * (1 - probs(3)) * (1 - probs(4)) * (1 - probs(5)) * (1 - probs(6)) * (1 - probs(7)) * (1 - probs(8)) * (1 - probs(9)) * (1 - probs(10)) * (1 - probs(11)) * (1 - probs(12)) * (1 - probs(13)) * (1 - probs(14)) * (1 - probs(15))
			// ***********************************************************
			double[] num = new double[CatCount];
			double[] den = new double[CatCount];
			double[] pct = new double[CatCount];

			// Initialize the array for the current range of possible categories
			for (int jj=0; jj<CatCount; jj++)
			{
				num[jj] = 1;
				den[jj] = 1;
				pct[jj] = 0;
			}
			
			try
			{
				// Apply Bayes Rule against the top tokens and across all categories
				for (int ii=0; ii<Math.Min(CategoryProbList.Count, Analyzer.MAXTOKENCHECK); ii++)
				{
					double ProbRaw = m_CheckCache.ProbabilityMaxMin(((CategoryProbEntry)(CategoryProbList[ii])).Probability);
					double ProbNot = (1.0 - ProbRaw) / (CatCount - 1);
					for (int jj=0; jj<CatCount; jj++)
					{
						// Split the "not this category" probability across the remaining group
						double ProbCat = (((CategoryProbEntry)(CategoryProbList[ii])).CatIndex == jj) ? ProbRaw : ProbNot;
						num[jj] = num[jj] * (ProbCat);
						den[jj] = den[jj] * (1 - ProbCat);
					}
				}
				// Finalize the calculation
				for (int jj=0; jj<CatCount; jj++)
				{
					den[jj] = den[jj] + num[jj];
					pct[jj] = m_CheckCache.ProbabilityMaxMin(num[jj] / den[jj]);
				}
			}
			catch(Exception ex)
			{
				m_Analyzer.FireLogException(ex, "CatCount", CatCount.ToString());
			}

			// Return probability for all categories
			return (pct);
		}
		#endregion

		#region TokenCache
		private class TokenCache : Hashtable
		{
			private Int64[] m_FrequencySum;

			#region Constructors
			public TokenCache (int CategoryCount)
			{
				m_FrequencySum = new Int64[CategoryCount];
			}
			#endregion

			#region Add (override)
			public override void Add(object key, object value)
			{
				if (value is TokenEntry)
				{
					// Update frequency sum
					TokenEntry TokenUpdate = (TokenEntry) value;
					for (int ii=0; ii<CategoryCount; ii++)
					{
						m_FrequencySum[ii] += TokenUpdate[ii];
					}
					// Add Token entry
					base.Add(key, value);
				}
			}
			#endregion

			public TokenEntry Update (string Token, TokenEntry TokenOriginal)
			{
				TokenEntry TokenUpdate = (TokenEntry) this[Token];
				if (TokenUpdate == null)
				{
					this[Token] = TokenOriginal;
				}
				else
				{
					for (int ii=0; ii<CategoryCount; ii++)
					{
						TokenUpdate[ii] += TokenOriginal[ii];
						TokenUpdate.LastReceived = TokenOriginal.LastReceived;
					}
				}
				// Update Frequency sums
				for (int ii=0; ii<CategoryCount; ii++)
				{
					m_FrequencySum[ii] += TokenOriginal[ii];
				}

				return (TokenUpdate);
			}
			public double ProbabilityMaxMin (double Probability)
			{
				const double PROBABILITYMIN = 0.01;
				const double PROBABILITYMAX = 0.99;
				return(Math.Min(Math.Max(Probability, PROBABILITYMIN), PROBABILITYMAX));
			}
			public Int64[] FrequencySum
			{
				get {return(m_FrequencySum);}
			}
			public int CategoryCount
			{
				get {return(m_FrequencySum.Length);}
			}
		}
		#endregion

		#region IsGoodToken
		private bool IsGoodToken (string word, out string Token)
		{
			const int TOK_MINLENGTH = 2;
			const int TOK_MAXLENGTH = 255;

			// Reject blanks
			Token = word.Trim();
			if (Token.Length < TOK_MINLENGTH) return (false);
				
			// Truncate if necessary
			Token = Token.Substring (0, System.Math.Min(Token.Length, TOK_MAXLENGTH));

			// Try to reject those guid looking things
			// AJM To Do

			// Fix any quotes (note that the length of the string remains the same)
			Token = EscapeQuotes(Token);

			// Token OK
			return (true);
		}
		#endregion

		#region ArrayValues
		private string ArrayValues (Double[] dValues)
		{
			string Text = "";
			for (int ii=0; ii<dValues.Length; ii++)
			{
				Text = Text + dValues[ii].ToString("0.0000") + "\n";
			}
			return (Text);
		}
		#endregion

		#region EscapeQuotes
		private string EscapeQuotes (string Text)
		{
			// Fix any quotes (note that the length of the string remains the same)
			return (Text.Replace("'", "`"));
		}
		#endregion

	}
}
