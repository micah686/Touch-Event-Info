using System.Data;
using System.Drawing;
using System.Linq;
public class ColorChangerWindowClick
{
  private void button1_Click(object sender, EventArgs e)
        {
            button1.BackColor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
            button1.Text = RandomString(50);
            //MessageBox.Show("button Clicked");
        }
        private static readonly Random rand = new Random();

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
}
