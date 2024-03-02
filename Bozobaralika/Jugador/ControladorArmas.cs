using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;

namespace Bozobaralika
{
    public class ControladorArmas : SyncScript
    {

        public override void Start()
        {

        }

        public override void Update()
        {
            if (Input.IsMouseButtonPressed(MouseButton.Left))
                Disparar();
        }

        private void Disparar()
        {

        }
    }
}
