using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashDriveApp.Extentions
{
    public enum FlashDriveState
    {
        Ready,
        NotEnoughSpace,
        Record,
        DeleteData,
        Error
    }
}
