﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Knapsack
{
	public partial class MainForm : Form
	{
		/// <summary>
		/// 构造器
		/// </summary>
		public MainForm()
		{
			InitializeComponent();
		}
		
		#region 控件合法性检查辅助函数
		private void trackBar1_Scroll(object sender, EventArgs e)
		{
			this.label7.Text = "背包尺寸占物体总重比：" + (this.trackBar1.Value * 10).ToString() + "%";
		}

		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			if (this.numericUpDown1.Value > this.numericUpDown2.Value) { this.numericUpDown1.Value = this.numericUpDown1.Minimum; }
		}

		private void numericUpDown2_ValueChanged(object sender, EventArgs e)
		{
			if (this.numericUpDown1.Value > this.numericUpDown2.Value) { this.numericUpDown2.Value = this.numericUpDown2.Maximum; }
		}

		private void numericUpDown4_ValueChanged(object sender, EventArgs e)
		{
			if (this.numericUpDown4.Value > this.numericUpDown3.Value) { this.numericUpDown4.Value = this.numericUpDown4.Minimum; }
		}

		private void numericUpDown3_ValueChanged(object sender, EventArgs e)
		{
			if (this.numericUpDown4.Value > this.numericUpDown3.Value) { this.numericUpDown3.Value = this.numericUpDown3.Maximum; }
		}

		private void numericUpDown6_ValueChanged(object sender, EventArgs e)
		{
			if (this.numericUpDown6.Value > this.numericUpDown5.Value) { this.numericUpDown6.Value = this.numericUpDown6.Minimum; }
		}

		private void numericUpDown5_ValueChanged(object sender, EventArgs e)
		{
			if (this.numericUpDown6.Value > this.numericUpDown5.Value) { this.numericUpDown5.Value = this.numericUpDown5.Maximum; }
		}
		#endregion
		
		/// <summary>
		/// 问题解决收尾处理
		/// </summary>
		/// <param name="solver">解决器</param>
		/// <param name="Rets">返回列表</param>
		private void PostSolve(Solver solver, ref Dictionary<string, string> Rets)
		{
			double Cost;
			var dr = MessageBox.Show("计算完毕，是否直接保存结果到TXT文件？" + Environment.NewLine 
				+ "（选择否将直接输出到UI，这将消耗一定的时间）", "信息", MessageBoxButtons.YesNo);
			if (dr == DialogResult.Yes)
			{
				solver.GetCost(out Cost);
				SaveFileDialog sfd = new SaveFileDialog();
				sfd.Filter = "txt|*.txt";
				DialogResult drr = sfd.ShowDialog();
				if (drr == DialogResult.Cancel || sfd.FileName == "")
				{
					MessageBox.Show("请指定合法的文件名！");
					return;
				}
				solver.GetResultFile(sfd.FileName);
				MessageBox.Show("保存完毕");
			}
			else
			{
				solver.GetResult(out Cost, out Rets);
			}
			this.cost_label.Text = String.Format("{0} ms", Cost.ToString("0.000000"));
		}

		/// <summary>
		/// 按钮：生成
		/// </summary>
		private void button1_Click(object sender, EventArgs e)
		{
			this.ProblemGen.Generate((int)this.numericUpDown1.Value, (int)this.numericUpDown2.Value,
				(int)this.numericUpDown4.Value, (int)this.numericUpDown3.Value,
				(int)this.numericUpDown6.Value, (int)this.numericUpDown5.Value,
				(this.trackBar1.Value) / 10.0);
			this.testProblemString = this.ProblemGen.Get();
			this.testStringDescriptor = this.ProblemGen.GetDescription();
		}

		/// <summary>
		/// 按钮：浏览
		/// </summary>
		private void button2_Click(object sender, EventArgs e)
		{
			PreviewForm pForm = new PreviewForm(this.testStringDescriptor, this.testProblemString);
			pForm.ShowDialog();
		}

		/// <summary>
		/// 按钮：保存
		/// </summary>
		private void button3_Click(object sender, EventArgs e)
		{
			SaveFileDialog sForm = new SaveFileDialog();
			sForm.Filter = "txt|*.txt";
			DialogResult dr = sForm.ShowDialog();
			if (dr == DialogResult.Cancel || sForm.FileName == "") { return; }
			this.ProblemGen.Save(sForm.FileName);
		}

		/// <summary>
		/// 按钮：导入测试
		/// </summary>
		private void button8_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "txt|*.txt";
			DialogResult dr = ofd.ShowDialog();
			if (dr == DialogResult.Cancel || ofd.FileName == "") { return; }
			FileStream fs = new FileStream(ofd.FileName, FileMode.Open);
			StreamReader sr = new StreamReader(fs);
			this.testProblemString = sr.ReadToEnd();
			this.testStringDescriptor = "- - - - - - -";
			sr.Close();
			fs.Close();
		}

		/// <summary>
		/// 调用问题解决器
		/// </summary>
		/// <param name="solver">解决器实例</param>
		/// <param name="methodName">算法名称</param>
		/// <param name="paras">参数列表</param>
		/// <returns>返回值字典</returns>
		private Dictionary<string, string> CallSolver(Solver solver, string methodName, params string[] paras)
		{
			if (this.testProblemString == String.Empty)
			{
				MessageBox.Show("请先生成测试数据");
				return null;
			}
			this.method_label.Text = "计算中…";
			Dictionary<string, string> Rets = new Dictionary<string, string>();
			solver.Init(this.output_textBox, paras);
			solver.Solve(this.testProblemString);
			this.method_label.Text = methodName;
			this.PostSolve(solver, ref Rets);
			return Rets;
		}
		
		/// <summary>
		/// 按钮：贪心算法
		/// </summary>
		private void button4_Click(object sender, EventArgs e)
		{
			this.CallSolver(new GreedySolver(), "贪心算法");
		}
		
        /// <summary>
        /// 按钮：动态规划
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
			this.CallSolver(new DynamicProgramSolver(), "动态规划");
		}

		/// <summary>
		/// 按钮：分支界限
		/// </summary>
		private void button6_Click(object sender, EventArgs e)
		{
			this.CallSolver(new BranchBoundSolver(), "分支界限");
		}

		/// <summary>
		/// 按钮：回溯
		/// </summary>
		private void button9_Click(object sender, EventArgs e)
		{
			this.CallSolver(new BackTraceSolver(), "回溯");
		}

		/// <summary>
		/// 按钮：模拟退火
		/// </summary>
		private void button7_Click(object sender, EventArgs e)
		{
			this.CallSolver(new SimulatedAnnealingSolver(), "模拟退火", this.numericUpDown7.Value.ToString());
		}

		/// <summary>
		/// 问题生成器引用
		/// </summary>
		private ProblemGenerator ProblemGen = ProblemGenerator.GetInstance();

		/// <summary>
		/// 生成的测试数据对应的字符串
		/// </summary>
		private string testProblemString = String.Empty;

		/// <summary>
		/// 生成的测试数据的描述
		/// </summary>
		private string testStringDescriptor = String.Empty;
	}
}
