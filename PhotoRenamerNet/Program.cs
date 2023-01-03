using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace PhotoRenamerNet
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        public static int FindMaxConsecutiveOnes(int[] nums)
        {
            var counts = new List<int>();

            var count = 0;

            if (nums.Length <= 0)
            {
                return 0;
            }

            if (nums.Length == 1)
            {
                return 1;
            }

            foreach (var num in nums)
            {
                if (num == 1)
                {
                    count++;
                }
                else
                {
                    counts.Add(count);
                    count = 0;
                }
            }

            if (count > 0)
            {
                counts.Add(count);
            }

            if (counts.Count == 1)
            {
                var res = counts.First();
                return res <= 1 ? 2 : res;
            }

            count = 0;
            var fs = 0;
            var ss = 0;
            for (var i = 0; i < counts.Count -1; i++)
            {
                var num = counts[i] + counts[i + 1];
                if (count < num)
                {
                    count = num;
                    fs = counts[i];
                    ss = counts[i + 1];
                }
            }

            return fs + ss + 1;
        }

        public static int[] ReplaceElements(int[] arr)
        {
            //[17,18,5,4,6,1] -> [18,6,6,6,1,-1]
            //[1,4,7,2,1,4] -> [7,7,4,4,4,-1]
            //[6,1,7,2,9,3] -> [9,9,9,9,3,-1]

            var max = arr.Max();
            var st = 0;
            while (max >= 0)
            {
                var list = arr.ToList();
                max = arr.Where(x => x <= max).Max();
                var ind = list.LastIndexOf(max);
                for (var i = st; i < ind; i++)
                {
                    if (arr[i] < max)
                    {
                        arr[i] = max;
                    }
                }

                if (ind >= arr.Length - 1)
                {
                    break;
                }
                max--;
                st = ind;
            }

            arr[arr.Length - 1] = -1;
            

            return arr;

        }

        public static bool ValidMountainArray(int[] arr)
        {
            int? topIndex = null;
            for (var i = 0; i < arr.Length - 1; i++)
            {
                if (!topIndex.HasValue)
                {
                    if (arr[i] < arr[i + 1])
                    {
                        continue;
                    }

                    if (arr[i] == arr[i + 1])
                    {
                        return false;
                    }
                    topIndex = i;
                }
                else
                {
                    if (arr[i] <= arr[i + 1])
                    {
                        return false;
                    }
                }
            }
            return topIndex > 0;
        }

        public static bool CheckIfExist(int[] arr) => arr.Where((t1, i) => arr.Where((t, j) => i != j).Any(t => t1 == t * 2)).Any();
        

        //public bool CheckIfExist(int[] arr)
        //{
        //    bool res = false;

        //    for (int i = 0; i < arr.Length; i++)
        //    {
        //        for (int j = 0; j < arr.Length; j++)
        //        {
        //            if (i == j)
        //            {
        //                continue;
        //            }
        //            if (arr[i] == arr[j] * 2)
        //            {
        //                res = true;
        //                break;
        //            }
        //        }
        //    }
        //    return res;
        //}
    }
}