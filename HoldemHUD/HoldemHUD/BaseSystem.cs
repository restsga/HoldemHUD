using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoldemHUD
{
    class BaseSystem
    {
        public static void ResetArray(ref int[] array)
        {
            ResetArray(ref array, 0);
        }
        public static void ResetArray(ref int[] array,int reset_number)
        {
            for(int i = 0; i < array.Length; i++)
            {
                array[i] = reset_number;
            }
        }
        public static void ResetArray(ref bool[] array)
        {
            ResetArray(ref array, false);
        }
        public static void ResetArray(ref bool[] array,bool value)
        {
            for(int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }

        public static bool AddArray(ref int[] array,int[] add)
        {
            if (array.Length != add.Length)
            {
                return false;
            }

            for(int i = 0; i < array.Length; i++)
            {
                array[i] += add[i];
            }

            return true;
        }

        public static int SumArray(int[] array)
        {
            int sum = 0;

            foreach(int x in array)
            {
                sum += x;
            }

            return sum;
        }
    }
}
