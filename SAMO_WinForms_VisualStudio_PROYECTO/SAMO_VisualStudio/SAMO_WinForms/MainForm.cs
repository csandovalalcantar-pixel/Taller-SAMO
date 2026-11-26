using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SAMO_WinForms
{
    public class MainForm : Form
    {
        private readonly Color navy = Color.FromArgb(25,54,80), blue = Color.FromArgb(28,85,149), red = Color.FromArgb(200,50,72);
        private Database db;
        private Panel side, content, top;
        private DataGridView grid;
        private string currentPage = "Inicio";

        public MainForm()
        {
            Text = "SAMO | Sistema Automotriz"; StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1280, 800); MinimumSize = new Size(1050, 680);
            try { db = new Database(); }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo conectar con SQL Server.\n\n" + ex.Message +
                    "\n\nRevisa App.config, que SQL Server esté iniciado y que existan las dos bases.", "Conexión",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Shown += (s,e) => Close(); return;
            }
            BuildShell(); ShowHome();
        }

        private void BuildShell()
        {
            side = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = navy };
            var logo = new Label { Text = "SAMO", ForeColor = Color.White, Font = new Font("Segoe UI", 25, FontStyle.Bold), AutoSize = true, Location = new Point(25,25) };
            side.Controls.Add(logo);
            var sub = new Label { Text = "CONTROL AUTOMOTRIZ", ForeColor = Color.LightSteelBlue, Font = new Font("Segoe UI",8,FontStyle.Bold), AutoSize=true, Location=new Point(28,68) };
            side.Controls.Add(sub);
            string[] menus = { "Inicio", "Clientes", "Vehículos", "Recepción", "Órdenes de servicio", "Inventario", "Ventas", "Historial" };
            int y = 125;
            foreach (string item in menus)
            {
                var b = new Button { Text = "   " + item, Tag = item, Location = new Point(12,y), Size = new Size(196,43),
                    FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.White,
                    BackColor = navy, Font = new Font("Segoe UI",10) };
                b.FlatAppearance.BorderSize = 0; b.FlatAppearance.MouseOverBackColor = Color.FromArgb(39,75,105);
                b.Click += (s,e) => Navigate((string)((Button)s).Tag); side.Controls.Add(b); y += 49;
            }
            top = new Panel { Dock = DockStyle.Top, Height = 65, BackColor = Color.White };
            content = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(242,244,248), Padding = new Padding(24) };
            Controls.Add(content); Controls.Add(top); Controls.Add(side);
        }

        private void Navigate(string page)
        {
            currentPage = page;
            switch(page)
            {
                case "Inicio": ShowHome(); break;
                case "Clientes": ShowClients(); break;
                case "Vehículos": ShowVehicles(); break;
                case "Recepción": ShowReception(); break;
                case "Órdenes de servicio": ShowOrders(); break;
                case "Inventario": ShowInventory(); break;
                case "Ventas": ShowSales(); break;
                case "Historial": ShowHistory(); break;
            }
        }
        private void Header(string title, string subtitle)
        {
            content.Controls.Clear(); top.Controls.Clear();
            top.Controls.Add(new Label { Text=title, Font=new Font("Segoe UI",17,FontStyle.Bold), ForeColor=navy, AutoSize=true, Location=new Point(25,16) });
            top.Controls.Add(new Label { Text=subtitle, Font=new Font("Segoe UI",9), ForeColor=Color.Gray, AutoSize=true, Location=new Point(27,43) });
        }
        private Button Btn(string text, Action click, bool primary=false)
        {
            var b = new Button { Text=text, Height=38, Width=150, BackColor=primary?red:Color.White, ForeColor=primary?Color.White:blue,
                FlatStyle=FlatStyle.Flat, Font=new Font("Segoe UI",9,FontStyle.Bold), Margin=new Padding(5) };
            b.FlatAppearance.BorderColor=Color.FromArgb(215,224,235); b.Click += (s,e)=>Safe(click); return b;
        }
        private void Safe(Action a) { try { a(); } catch(Exception ex) { MessageBox.Show(ex.Message,"SAMO",MessageBoxButtons.OK,MessageBoxIcon.Warning); } }
        private Label TextLabel(string text, int size=10, bool bold=false)
        { return new Label { Text=text, AutoSize=true, ForeColor=Color.FromArgb(38,57,76), Font=new Font("Segoe UI",size,bold?FontStyle.Bold:FontStyle.Regular), Margin=new Padding(5,10,5,5) }; }
        private void AddGrid(DataTable dt)
        {
            if(grid!=null) grid.Dispose();
            grid = new DataGridView { Dock=DockStyle.Fill, ReadOnly=true, AllowUserToAddRows=false, AllowUserToDeleteRows=false,
                AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill, SelectionMode=DataGridViewSelectionMode.FullRowSelect,
                MultiSelect=false, BackgroundColor=Color.White, BorderStyle=BorderStyle.None, RowHeadersVisible=false,
                Font=new Font("Segoe UI",9), DataSource=dt };
            grid.ColumnHeadersDefaultCellStyle.BackColor=Color.FromArgb(234,240,247);
            grid.ColumnHeadersDefaultCellStyle.ForeColor=navy; grid.EnableHeadersVisualStyles=false;
            grid.RowTemplate.Height=34; content.Controls.Add(grid); grid.BringToFront();
        }
        private int SelectedId(string column)
        {
            if(grid==null || grid.CurrentRow==null) throw new Exception("Selecciona un registro de la tabla.");
            return Convert.ToInt32(grid.CurrentRow.Cells[column].Value);
        }
        private string Ask(string title,string label,string initial="")
        {
            using(var f=new Form { Text=title, Size=new Size(430,170), StartPosition=FormStartPosition.CenterParent, FormBorderStyle=FormBorderStyle.FixedDialog, MaximizeBox=false, MinimizeBox=false })
            {
                var l=new Label{Text=label,Left=18,Top=15,Width=370}; var t=new TextBox{Left=18,Top=43,Width=370,Text=initial};
                var ok=new Button{Text="Aceptar",DialogResult=DialogResult.OK,Left=290,Top=83,Width=95};
                f.Controls.AddRange(new Control[]{l,t,ok}); f.AcceptButton=ok;
                return f.ShowDialog(this)==DialogResult.OK?t.Text:null;
            }
        }
        private string[] AskMany(string title, params string[] labels)
        {
            using(var f=new Form { Text=title, Size=new Size(450, 95+labels.Length*62), StartPosition=FormStartPosition.CenterParent, FormBorderStyle=FormBorderStyle.FixedDialog, MaximizeBox=false, MinimizeBox=false })
            {
                var boxes=new TextBox[labels.Length];
                for(int i=0;i<labels.Length;i++){ f.Controls.Add(new Label{Text=labels[i],Left=18,Top=15+i*58,Width=180}); boxes[i]=new TextBox{Left=190,Top=12+i*58,Width=225}; f.Controls.Add(boxes[i]); }
                var ok=new Button{Text="Guardar",DialogResult=DialogResult.OK,Left=320,Top=25+labels.Length*58,Width=95}; f.Controls.Add(ok); f.AcceptButton=ok;
                if(f.ShowDialog(this)!=DialogResult.OK)return null;
                return boxes.Select(x=>x.Text.Trim()).ToArray();
            }
        }
        private void ShowHome()
        {
            Header("Panel principal","Resumen general de SAMO · Taller y refaccionaria");
            var wrap=new FlowLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(12),AutoScroll=true};
            content.Controls.Add(wrap);
            decimal recaudado=0; int ordenes=0, productos=0, vehiculos=0;
            try { recaudado=Convert.ToDecimal(db.Scalar("SELECT COALESCE(SUM(total),0) FROM VENTA_REFACCION WHERE estado='Pagada' AND CAST(fecha AS DATE)=CAST(GETDATE() AS DATE)",false)); } catch{}
            try { ordenes=Convert.ToInt32(db.Scalar("SELECT COUNT(*) FROM ORDEN_SERVICIO WHERE estado IN ('Abierta','En proceso')",true)); } catch{}
            try { productos=Convert.ToInt32(db.Scalar("SELECT COUNT(*) FROM REFACCION",false)); } catch{}
            try { vehiculos=Convert.ToInt32(db.Scalar("SELECT COUNT(*) FROM VEHICULO",true)); } catch{}
            AddStat(wrap,"Ventas mostrador de hoy",recaudado.ToString("C2"));
            AddStat(wrap,"Órdenes abiertas",ordenes.ToString());
            AddStat(wrap,"Refacciones registradas",productos.ToString());
            AddStat(wrap,"Vehículos registrados",vehiculos.ToString());
            var note=new Label{Text="Sistema de gestión para taller automotriz y refaccionaria.\n\nSelecciona un módulo en el menú izquierdo para comenzar.",AutoSize=true,Font=new Font("Segoe UI",12),ForeColor=navy,Margin=new Padding(15,30,15,10)};
            wrap.Controls.Add(note);
        }
        private void AddStat(FlowLayoutPanel p,string title,string value)
        {
            var c=new Panel{Width=245,Height=120,BackColor=Color.White,Margin=new Padding(10),Padding=new Padding(15)};
            c.Controls.Add(new Label{Text=title,AutoSize=true,ForeColor=Color.Gray,Location=new Point(14,15),Font=new Font("Segoe UI",9)});
            c.Controls.Add(new Label{Text=value,AutoSize=true,ForeColor=navy,Location=new Point(14,48),Font=new Font("Segoe UI",22,FontStyle.Bold)}); p.Controls.Add(c);
        }
        private void Toolbar(params Control[] controls)
        { var bar=new FlowLayoutPanel{Dock=DockStyle.Top,Height=52,BackColor=Color.Transparent}; bar.Controls.AddRange(controls); content.Controls.Add(bar); bar.BringToFront(); }
        private void ShowClients()
        {
            Header("Clientes","Directorio de clientes registrados");
            Toolbar(Btn("＋ Nuevo cliente",()=>NewClient(),true),Btn("↻ Actualizar",()=>ShowClients()));
            AddGrid(db.Query("SELECT id_cliente,nombre,telefono,correo,direccion FROM CLIENTE ORDER BY nombre",true));
        }
        private void NewClient()
        {
            var a=AskMany("Nuevo cliente","Nombre completo","Teléfono","Correo","Dirección"); if(a==null)return;
            db.Exec("INSERT INTO CLIENTE(nombre,telefono,correo,direccion) VALUES(?,?,?,?)",true,a);
            ShowClients();
        }
        private void ShowVehicles()
        {
            Header("Vehículos","Vehículos registrados y sus propietarios");
            Toolbar(Btn("＋ Nuevo vehículo",()=>NewVehicle(),true),Btn("↻ Actualizar",()=>ShowVehicles()));
            AddGrid(db.Query(@"SELECT v.id_vehiculo,v.placa,v.marca,v.modelo,v.anio,c.nombre AS cliente,c.telefono
                FROM VEHICULO v INNER JOIN CLIENTE c ON c.id_cliente=v.id_cliente ORDER BY v.id_vehiculo DESC",true));
        }
        private void NewVehicle()
        {
            var clients=db.Query("SELECT id_cliente,nombre FROM CLIENTE ORDER BY nombre",true);
            if(clients.Rows.Count==0)throw new Exception("Primero registra un cliente.");
            using(var f=new Form{Text="Nuevo vehículo",Size=new Size(460,330),StartPosition=FormStartPosition.CenterParent,FormBorderStyle=FormBorderStyle.FixedDialog})
            {
                var cb=new ComboBox{Left=155,Top=18,Width=250,DropDownStyle=ComboBoxStyle.DropDownList,DataSource=clients,DisplayMember="nombre",ValueMember="id_cliente"};
                string[] labs={"Cliente","Placa","Marca","Modelo","Año"}; var tx=new TextBox[4];
                f.Controls.Add(new Label{Text=labs[0],Left=20,Top=22,Width=120}); f.Controls.Add(cb);
                for(int i=1;i<5;i++){f.Controls.Add(new Label{Text=labs[i],Left=20,Top=65+(i-1)*43,Width=120});tx[i-1]=new TextBox{Left=155,Top=61+(i-1)*43,Width=250};f.Controls.Add(tx[i-1]);}
                var ok=new Button{Text="Guardar",Left=310,Top=280,DialogResult=DialogResult.OK};f.Controls.Add(ok);f.AcceptButton=ok;
                if(f.ShowDialog(this)!=DialogResult.OK)return;
                db.Exec("INSERT INTO VEHICULO(id_cliente,placa,marca,modelo,anio) VALUES(?,?,?,?,?)",true,cb.SelectedValue,tx[0].Text,tx[1].Text,tx[2].Text,Convert.ToInt32(tx[3].Text));
            }
            ShowVehicles();
        }
        private void ShowReception()
        {
            Header("Recepción","Registrar la llegada de un vehículo al taller");
            Toolbar(Btn("＋ Recibir vehículo",()=>ReceiveVehicle(),true),Btn("Ver vehículos",()=>ShowVehicles()));
            AddGrid(db.Query(@"SELECT v.id_vehiculo,v.placa,v.marca,v.modelo,v.anio,c.nombre AS cliente,c.telefono
                FROM VEHICULO v INNER JOIN CLIENTE c ON c.id_cliente=v.id_cliente ORDER BY c.nombre",true));
        }
        private void ReceiveVehicle()
        {
            int vid=SelectedId("id_vehiculo"); string work=Ask("Recepción","Trabajo solicitado / motivo");
            if(work==null)return;
            object open=db.Scalar(@"SELECT TOP 1 os.id_orden FROM ORDEN_SERVICIO os INNER JOIN CITA ct ON ct.id_cita=os.id_cita
                WHERE ct.id_vehiculo=? AND os.estado IN('Abierta','En proceso')",true,vid);
            if(open!=null && open!=DBNull.Value)throw new Exception("Este vehículo ya tiene una orden abierta: OS-"+Convert.ToInt32(open).ToString("0000"));
            int cita=db.InsertId("INSERT INTO CITA(id_vehiculo,fecha_hora,motivo,estado) VALUES(?,GETDATE(),?,'Atendida')",true,vid,work);
            int os=db.InsertId("INSERT INTO ORDEN_SERVICIO(id_cita,fecha_ingreso,costo_servicios,costo_refacciones,total,estado_pago,estado) VALUES(?,GETDATE(),0,0,0,'Pendiente','Abierta')",true,cita);
            MessageBox.Show("Orden creada: OS-"+os.ToString("0000")); ShowOrders();
        }
        private void ShowOrders()
        {
            Header("Órdenes de servicio","Seguimiento de trabajos, refacciones y pagos");
            Toolbar(Btn("＋ Nueva recepción",()=>ShowReception(),true),Btn("↻ Actualizar",()=>ShowOrders()),Btn("Agregar servicio",()=>AddService()),Btn("Agregar refacción",()=>AddPart()),Btn("Cambiar estado",()=>ChangeOrderState()));
            AddGrid(db.Query(@"SELECT os.id_orden,os.fecha_ingreso,os.estado,os.estado_pago,os.costo_servicios,os.costo_refacciones,os.total,
                v.placa,v.marca,v.modelo,c.nombre AS cliente,ct.motivo
                FROM ORDEN_SERVICIO os JOIN CITA ct ON ct.id_cita=os.id_cita JOIN VEHICULO v ON v.id_vehiculo=ct.id_vehiculo
                JOIN CLIENTE c ON c.id_cliente=v.id_cliente ORDER BY os.id_orden DESC",true));
        }
        private void AddService()
        {
            int id=SelectedId("id_orden"); var a=AskMany("Agregar servicio","Descripción del trabajo","Precio","Observaciones");if(a==null)return;
            decimal price=decimal.Parse(a[1]);
            db.Exec("INSERT INTO DETALLE_SERVICIO(id_orden,descripcion_trabajo,precio,observaciones) VALUES(?,?,?,?)",true,id,a[0],price,a[2]);
            Recalc(id); ShowOrders();
        }
        private void AddPart()
        {
            int id=SelectedId("id_orden");
            var products=db.Query("SELECT id_refaccion,codigo,descripcion,precio_venta,existencia FROM REFACCION ORDER BY descripcion",false);
            using(var f=new Form{Text="Agregar refacción a la orden",Size=new Size(570,220),StartPosition=FormStartPosition.CenterParent,FormBorderStyle=FormBorderStyle.FixedDialog})
            {
                var cb=new ComboBox{Left=20,Top=25,Width=510,DropDownStyle=ComboBoxStyle.DropDownList,DataSource=products,DisplayMember="descripcion",ValueMember="id_refaccion"};
                var q=new TextBox{Left=120,Top=75,Width=100,Text="1"};f.Controls.Add(cb);f.Controls.Add(new Label{Text="Cantidad",Left=20,Top=78});f.Controls.Add(q);
                var ok=new Button{Text="Agregar",Left=430,Top=125,DialogResult=DialogResult.OK};f.Controls.Add(ok);f.AcceptButton=ok;
                if(f.ShowDialog(this)!=DialogResult.OK)return;
                int pid=Convert.ToInt32(cb.SelectedValue),qty=Convert.ToInt32(q.Text);
                var pr=db.Query("SELECT precio_venta,existencia FROM REFACCION WHERE id_refaccion=?",false,pid).Rows[0];
                if(Convert.ToInt32(pr["existencia"])<qty)throw new Exception("No hay existencia suficiente.");
                decimal unit=Convert.ToDecimal(pr["precio_venta"]),sub=unit*qty;
                db.Exec("UPDATE REFACCION SET existencia=existencia-? WHERE id_refaccion=?",false,qty,pid);
                db.Exec("INSERT INTO MOVIMIENTO_INVENTARIO(id_refaccion,fecha,tipo,cantidad,motivo) VALUES(?,GETDATE(),'Salida',?,?)",false,pid,qty,"Consumo en orden OS-"+id.ToString("0000"));
                db.Exec("INSERT INTO SOLICITUD_REFACCION(id_orden,id_refaccion,cantidad,precio_unitario,subtotal,fecha_solicitud,estado) VALUES(?,?,?,?,?,GETDATE(),'Surtida')",true,id,pid,qty,unit,sub);
                Recalc(id);
            }
            ShowOrders();
        }
        private void Recalc(int id)
        {
            db.Exec(@"UPDATE ORDEN_SERVICIO SET costo_servicios=ISNULL((SELECT SUM(precio) FROM DETALLE_SERVICIO WHERE id_orden=?),0),
                costo_refacciones=ISNULL((SELECT SUM(subtotal) FROM SOLICITUD_REFACCION WHERE id_orden=? AND estado<>'Cancelada'),0),
                total=ISNULL((SELECT SUM(precio) FROM DETALLE_SERVICIO WHERE id_orden=?),0)+
                ISNULL((SELECT SUM(subtotal) FROM SOLICITUD_REFACCION WHERE id_orden=? AND estado<>'Cancelada'),0) WHERE id_orden=?",true,id,id,id,id,id);
        }
        private void ChangeOrderState()
        {
            int id=SelectedId("id_orden");
            string state=Ask("Cambiar estado","Escribe: En proceso, Terminada o Cancelada");
            if(state==null)return;
            if(state=="Terminada")
            {
                Recalc(id);
                object total=db.Scalar("SELECT total FROM ORDEN_SERVICIO WHERE id_orden=?",true,id);
                if(Convert.ToDecimal(total)<=0)throw new Exception("La orden debe tener un total mayor a cero.");
                db.Exec("UPDATE ORDEN_SERVICIO SET estado=? WHERE id_orden=?",true,state,id);
            }
            else if(state=="Cancelada")
            {
                var parts=db.Query("SELECT id_refaccion,cantidad,id_solicitud FROM SOLICITUD_REFACCION WHERE id_orden=? AND estado<>'Cancelada'",true,id);
                foreach(DataRow p in parts.Rows)
                {
                    int pid=Convert.ToInt32(p["id_refaccion"]),q=Convert.ToInt32(p["cantidad"]);
                    db.Exec("UPDATE REFACCION SET existencia=existencia+? WHERE id_refaccion=?",false,q,pid);
                    db.Exec("INSERT INTO MOVIMIENTO_INVENTARIO(id_refaccion,fecha,tipo,cantidad,motivo) VALUES(?,GETDATE(),'Entrada',?,?)",false,pid,q,"Cancelación orden OS-"+id.ToString("0000"));
                    db.Exec("UPDATE SOLICITUD_REFACCION SET estado='Cancelada' WHERE id_solicitud=?",true,p["id_solicitud"]);
                }
                Recalc(id); db.Exec("UPDATE ORDEN_SERVICIO SET estado='Cancelada' WHERE id_orden=?",true,id);
            }
            else if(state=="En proceso") db.Exec("UPDATE ORDEN_SERVICIO SET estado=? WHERE id_orden=?",true,state,id);
            else throw new Exception("Estado no válido.");
            ShowOrders();
        }
        private void ShowInventory()
        {
            Header("Inventario","Catálogo y existencias de refacciones");
            Toolbar(Btn("＋ Nueva refacción",()=>NewProduct(),true),Btn("Entrada de stock",()=>StockEntry()),Btn("↻ Actualizar",()=>ShowInventory()));
            AddGrid(db.Query("SELECT id_refaccion,codigo,descripcion,precio_venta,existencia,stock_minimo,stock_maximo FROM REFACCION ORDER BY descripcion",false));
        }
        private void NewProduct()
        {
            var a=AskMany("Nueva refacción","Código","Descripción","Precio de venta","Stock mínimo","Stock máximo","Existencia inicial");if(a==null)return;
            int id=db.InsertId("INSERT INTO REFACCION(codigo,descripcion,precio_venta,existencia,stock_minimo,stock_maximo) VALUES(?,?,?,0,?,?)",false,a[0],a[1],decimal.Parse(a[2]),int.Parse(a[3]),int.Parse(a[4]));
            int qty=int.Parse(a[5]); if(qty>0){db.Exec("UPDATE REFACCION SET existencia=existencia+? WHERE id_refaccion=?",false,qty,id);db.Exec("INSERT INTO MOVIMIENTO_INVENTARIO(id_refaccion,fecha,tipo,cantidad,motivo) VALUES(?,GETDATE(),'Entrada',?,'Inventario inicial')",false,id,qty);}
            ShowInventory();
        }
        private void StockEntry()
        {
            int id=SelectedId("id_refaccion"); var a=AskMany("Entrada de inventario","Cantidad","Motivo");if(a==null)return;
            int q=int.Parse(a[0]);if(q<1)throw new Exception("Cantidad no válida.");
            db.Exec("UPDATE REFACCION SET existencia=existencia+? WHERE id_refaccion=?",false,q,id);
            db.Exec("INSERT INTO MOVIMIENTO_INVENTARIO(id_refaccion,fecha,tipo,cantidad,motivo) VALUES(?,GETDATE(),'Entrada',?,?)",false,id,q,a[1]);
            ShowInventory();
        }
        private void ShowSales()
        {
            Header("Ventas de mostrador","Venta directa de refacciones");
            Toolbar(Btn("＋ Nueva venta",()=>NewSale(),true),Btn("Historial",()=>ShowHistory()));
            AddGrid(db.Query("SELECT id_refaccion,codigo,descripcion,precio_venta,existencia FROM REFACCION ORDER BY descripcion",false));
        }
        private void NewSale()
        {
            var products=db.Query("SELECT id_refaccion,codigo,descripcion,precio_venta,existencia FROM REFACCION WHERE existencia>0 ORDER BY descripcion",false);
            if(products.Rows.Count==0)throw new Exception("No hay refacciones disponibles.");
            using(var f=new Form{Text="Venta de mostrador",Size=new Size(640,430),StartPosition=FormStartPosition.CenterParent})
            {
                var cb=new ComboBox{Left=20,Top=25,Width=580,DropDownStyle=ComboBoxStyle.DropDownList,DataSource=products,DisplayMember="descripcion",ValueMember="id_refaccion"};
                var qty=new TextBox{Left=120,Top=70,Width=80,Text="1"};var client=new TextBox{Left=120,Top=110,Width=300,Text="Público general"};
                var method=new ComboBox{Left=120,Top=150,Width=180,DropDownStyle=ComboBoxStyle.DropDownList};method.Items.AddRange(new object[]{"Efectivo","Tarjeta","Transferencia"});method.SelectedIndex=0;
                f.Controls.AddRange(new Control[]{cb,qty,client,method,new Label{Text="Refacción",Left=20,Top=28},new Label{Text="Cantidad",Left=20,Top=73},new Label{Text="Cliente",Left=20,Top=113},new Label{Text="Método de pago",Left=20,Top=153}});
                var ok=new Button{Text="Cobrar",Left=480,Top=320,Width=120,Height=38,DialogResult=DialogResult.OK};f.Controls.Add(ok);f.AcceptButton=ok;
                if(f.ShowDialog(this)!=DialogResult.OK)return;
                int pid=Convert.ToInt32(cb.SelectedValue),q=int.Parse(qty.Text);
                var p=db.Query("SELECT precio_venta,existencia FROM REFACCION WHERE id_refaccion=?",false,pid).Rows[0];
                if(q<1 || Convert.ToInt32(p["existencia"])<q)throw new Exception("Cantidad no válida o stock insuficiente.");
                decimal total=Convert.ToDecimal(p["precio_venta"])*q;
                decimal received=total;
                if(method.Text=="Efectivo"){string r=Ask("Pago en efectivo","Importe recibido",total.ToString("0.00"));if(r==null)return;received=decimal.Parse(r);if(received<total)throw new Exception("El efectivo no cubre el total.");}
                int sale=db.InsertId("INSERT INTO VENTA_REFACCION(fecha,cliente,total,estado) VALUES(GETDATE(),?,?, 'Pagada')",false,string.IsNullOrWhiteSpace(client.Text)?"Público general":client.Text,total);
                db.Exec("INSERT INTO DETALLE_VENTA_REFACCION(id_venta,id_refaccion,cantidad,precio_unitario) VALUES(?,?,?,?)",false,sale,pid,q,p["precio_venta"]);
                db.Exec("UPDATE REFACCION SET existencia=existencia-? WHERE id_refaccion=?",false,q,pid);
                db.Exec("INSERT INTO MOVIMIENTO_INVENTARIO(id_refaccion,fecha,tipo,cantidad,motivo) VALUES(?,GETDATE(),'Salida',?,?)",false,pid,q,"Venta mostrador VEN-"+sale.ToString("0000"));
                db.Exec("INSERT INTO PAGO_REFACCION(id_venta,fecha,importe,metodo_pago,estado) VALUES(?,GETDATE(),?,?,'Aplicado')",false,sale,total,method.Text);
                MessageBox.Show("Venta registrada: VEN-"+sale.ToString("0000")+"\nTotal: "+total.ToString("C2")+"\nCambio: "+(received-total).ToString("C2"));
            }
            ShowSales();
        }
        private void ShowHistory()
        {
            Header("Historial de ventas","Operaciones registradas en la refaccionaria y el taller");
            Toolbar(Btn("↻ Actualizar",()=>ShowHistory()));
            var dt=db.Query(@"SELECT 'VEN-' + RIGHT('0000'+CAST(v.id_venta AS VARCHAR),4) AS folio,v.fecha,'Mostrador' AS modalidad,v.cliente,v.total,v.estado
                FROM VENTA_REFACCION v WHERE v.estado='Pagada' ORDER BY v.fecha DESC",false);
            AddGrid(dt);
        }
    }
}
