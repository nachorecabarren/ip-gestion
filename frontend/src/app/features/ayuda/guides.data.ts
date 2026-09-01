export interface GuideStep {
  title: string;
  detail?: string;
}

export interface Guide {
  id: string;
  category: string;
  title: string;
  summary: string;
  steps: GuideStep[];
  tips?: string[];
}

export const GUIDE_CATEGORIES = [
  'Primeros pasos',
  'Ventas',
  'Stock',
  'Compras',
  'Reservas',
  'Cajas',
  'Cuentas Corrientes',
  'Servicio Técnico',
  'Dashboard',
  'Equipo',
] as const;

export const GUIDES: Guide[] = [
  // ─── PRIMEROS PASOS ────────────────────────────────────────
  {
    id: 'navegacion',
    category: 'Primeros pasos',
    title: 'Cómo moverte por el sistema',
    summary: 'El menú, los accesos rápidos y dónde encontrar cada cosa.',
    steps: [
      {
        title: 'Usá el menú de la izquierda para ir entre módulos',
        detail:
          'Está agrupado en 4 secciones: Principal (Dashboard, Ventas, Compras, Reservas, Stock), Administración (Cajas, Base de Datos, Servicio Técnico, Cuentas Corrientes), Marketing (Retención, Agenda) y Sistema (Mi Equipo, Configuración). Algunos ítems de Sistema solo los ve el dueño de la cuenta.',
      },
      {
        title: 'Entrá al Dashboard para tener una foto general del negocio',
        detail:
          'Muestra facturación, margen, ganancia neta, cantidad de ventas, canjes recibidos y stock disponible del período que elijas (semana, mes o año), separados en dos pestañas: Financiero y Operativo.',
      },
      {
        title: 'Usá los accesos rápidos del Dashboard para ir directo a cargar algo',
        detail:
          '"Nueva Venta", "Nueva Compra", "Nueva Reserva" y "Nuevo Movimiento" abren el formulario correspondiente al toque, sin que tengas que entrar primero al módulo y buscar el botón.',
      },
    ],
    tips: [
      'En pantallas chicas (celular/tablet) el menú se convierte en un botón de hamburguesa arriba a la izquierda.',
      'Todos los formularios de alta se cierran con la tecla Escape, el botón ✕ o "Cancelar" — si ya cargaste datos, el sistema te va a pedir confirmación antes de descartarlos.',
    ],
  },

  // ─── VENTAS ────────────────────────────────────────────────
  {
    id: 'venta-nueva',
    category: 'Ventas',
    title: 'Cómo registrar una venta',
    summary: 'El asistente de 4 pasos: cliente, equipos, canje y pago.',
    steps: [
      {
        title: 'Entrá a Ventas y hacé clic en "+ Nueva Venta"',
        detail:
          'También podés usar el acceso rápido "Nueva Venta" del Dashboard — te lleva directo a Ventas con el asistente ya abierto.',
      },
      {
        title: 'Paso 1 — Cliente',
        detail:
          'Elegí la fecha de la venta, si es Minorista o Mayorista, y si el cliente es "Consumidor Final" (cargás nombre y WhatsApp sueltos) o un cliente ya registrado en tu Base de Datos.',
      },
      {
        title: 'Paso 2 — Equipos',
        detail:
          'Agregá uno o más ítems: buscá el equipo escaneando el IMEI o eligiéndolo del listado de stock disponible. El precio se completa solo con el precio sugerido (o mayorista, si corresponde), y podés ajustarlo a mano.',
      },
      {
        title: 'Paso 3 — Canje (opcional)',
        detail:
          'Si el cliente entrega un equipo usado como parte de pago, activá "¿Incluye canje?" y completá el modelo, IMEI, color, almacenamiento, batería, condición, el valor que le reconocés y el precio sugerido de reventa. Ver la guía "Cómo funciona el canje / trade-in" para el detalle completo.',
      },
      {
        title: 'Paso 4 — Pago',
        detail:
          'Agregá uno o más métodos de pago (efectivo USD, USDT, efectivo ARS, transferencia, MercadoPago) hasta cubrir el total a pagar (que ya descuenta el valor del canje si lo hubo).',
      },
      {
        title: 'Confirmá la venta',
        detail:
          'El stock de los equipos vendidos se descuenta automáticamente. Si hubo canje, el equipo recibido se da de alta solo en Stock, disponible para la próxima venta.',
      },
    ],
    tips: [
      'Solo el dueño de la cuenta puede anular una venta ya confirmada.',
    ],
  },
  {
    id: 'venta-canje',
    category: 'Ventas',
    title: 'Cómo funciona el canje / trade-in',
    summary: 'Qué pasa con el equipo que te entrega el cliente.',
    steps: [
      {
        title: 'Activá el canje en el paso 3 del asistente de venta',
        detail: 'Es un toggle "¿Incluye canje / trade-in?" — al activarlo aparece el formulario del equipo recibido.',
      },
      {
        title: 'Elegí el modelo del equipo del listado',
        detail: 'Es el mismo catálogo que usás en Compras y Stock — no hace falta escribirlo a mano.',
      },
      {
        title: 'Escaneá o ingresá el IMEI (opcional pero recomendado)',
        detail: 'Así el equipo queda identificado igual que cualquier otro de tu stock, y podés rastrearlo más adelante.',
      },
      {
        title: 'Completá color, almacenamiento, batería y condición',
        detail: 'La condición usa las mismas categorías que en Compras/Stock: Nuevo, Usado, Reacondicionado, A+, A, B.',
      },
      {
        title: 'Ingresá el valor del canje y el precio sugerido de reventa',
        detail:
          'El "valor del canje" es lo que le descontás al cliente del total de la venta. El "precio sugerido de reventa" es el precio con el que ese equipo va a aparecer en Stock una vez que confirmes la venta.',
      },
      {
        title: 'Confirmá la venta',
        detail:
          'El sistema da de alta el equipo recibido como un ítem de Stock disponible, con el valor del canje como costo — no tenés que cargarlo de nuevo a mano en Stock.',
      },
    ],
    tips: [
      'Podés ver el historial completo de equipos recibidos por canje en Stock → pestaña "Canjes".',
    ],
  },
  {
    id: 'venta-anular',
    category: 'Ventas',
    title: 'Cómo anular una venta',
    summary: 'Reversa el stock y las comisiones de una venta ya confirmada.',
    steps: [
      {
        title: 'Entrá a Ventas y ubicá la venta en el listado',
        detail: 'Podés filtrar por estado, categoría o buscar por cliente.',
      },
      {
        title: 'Hacé clic en el ícono "Anular" de esa fila',
        detail: 'Esta opción solo la ve el dueño de la cuenta, y solo aparece en ventas con estado "Completada".',
      },
      {
        title: 'Confirmá la anulación',
        detail: 'Es una acción irreversible: el stock vendido vuelve a estar disponible y las comisiones asociadas se revierten.',
      },
    ],
  },

  // ─── STOCK ─────────────────────────────────────────────────
  {
    id: 'stock-agregar',
    category: 'Stock',
    title: 'Cómo cargar un equipo en Stock',
    summary: 'Alta rápida de un equipo, con trazabilidad de proveedor.',
    steps: [
      {
        title: 'Entrá a Stock → pestaña "Equipos (Unitario)"',
        detail: '',
      },
      {
        title: 'Hacé clic en "+ Agregar equipo"',
        detail: 'Esta acción se registra como una compra de un solo ítem, igual que si la cargaras desde Compras → Equipos.',
      },
      {
        title: 'Completá proveedor (opcional), modelo, IMEI/serie, color, almacenamiento, condición y batería',
        detail: '',
      },
      {
        title: 'Cargá el costo y el precio sugerido de venta',
        detail: 'Opcionalmente también el precio mayorista y la ubicación física del equipo.',
      },
      {
        title: 'Guardá — el equipo queda disponible para la venta al instante',
        detail: '',
      },
    ],
    tips: [
      'Si vas a cargar varios equipos de una misma compra a la vez, es más rápido usar Compras → Nueva Compra en vez de repetir este paso equipo por equipo.',
    ],
  },
  {
    id: 'stock-accesorios',
    category: 'Stock',
    title: 'Cómo gestionar accesorios (stock bulk)',
    summary: 'A diferencia de los equipos, los accesorios se manejan por cantidad, no por IMEI.',
    steps: [
      {
        title: 'Entrá a Stock → pestaña "Accesorios (Bulk)"',
        detail: '',
      },
      {
        title: 'Revisá la columna "Alerta stock"',
        detail: 'Los accesorios con cantidad por debajo de su umbral configurado se marcan con la etiqueta "Stock bajo".',
      },
      {
        title: 'Para sumar cantidad, cargalos desde Compras → Nueva Compra → pestaña Accesorios',
        detail: 'Si ya existe un accesorio con el mismo color, la cantidad se suma automáticamente en vez de crear un duplicado.',
      },
    ],
  },
  // Cotizador de Canje deshabilitado por el momento (no se usa) — guía oculta hasta que se reactive.
  // {
  //   id: 'stock-cotizador',
  //   category: 'Stock',
  //   title: 'Cómo usar el cotizador de canje',
  //   summary: 'Calculá un valor de referencia antes de aceptar un canje.',
  //   steps: [
  //     {
  //       title: 'Entrá a Stock → pestaña "Canjes" y hacé clic en "Cotizador"',
  //       detail: '',
  //     },
  //     {
  //       title: 'Ingresá modelo, almacenamiento y % de batería del equipo',
  //       detail: '',
  //     },
  //     {
  //       title: 'Obtené el valor base y el valor ajustado',
  //       detail: 'Si la batería está por debajo del 80%, el sistema aplica automáticamente un descuento sobre el valor base.',
  //     },
  //   ],
  //   tips: [
  //     'El cotizador es solo una referencia — el valor final del canje se carga a mano en el paso 3 del asistente de venta.',
  //   ],
  // },
  {
    id: 'stock-historial-canjes',
    category: 'Stock',
    title: 'Cómo revisar el historial de canjes recibidos',
    summary: 'Qué equipos entraron por canje, de quién y por cuánto.',
    steps: [
      {
        title: 'Entrá a Stock → pestaña "Canjes"',
        detail: '',
      },
      {
        title: 'Revisá la tabla: fecha, cliente, modelo, IMEI, almacenamiento, batería y valor recibido',
        detail: 'La columna "En stock" confirma que el equipo quedó cargado como ítem disponible para la venta.',
      },
    ],
  },

  // ─── COMPRAS ───────────────────────────────────────────────
  {
    id: 'compra-nueva',
    category: 'Compras',
    title: 'Cómo registrar una compra a un proveedor',
    summary: 'Carga equipos y/o accesorios de una sola vez, con trazabilidad de costo.',
    steps: [
      {
        title: 'Entrá a Compras y hacé clic en "+ Nueva Compra"',
        detail: 'También podés usar el acceso rápido "Nueva Compra" del Dashboard.',
      },
      {
        title: 'Elegí el proveedor (opcional) y la fecha de la compra',
        detail: '',
      },
      {
        title: 'Cargá los equipos en la subpestaña "Equipos"',
        detail: 'Podés agregar varios equipos, cada uno con su modelo, IMEI, color, condición, costo y precio de venta. Usá "+ Agregar" para sumar más filas.',
      },
      {
        title: 'O cargá accesorios en la subpestaña "Accesorios (Bulk)"',
        detail: 'Elegí el accesorio del catálogo, la cantidad, costo y precio de venta.',
      },
      {
        title: 'Guardá la compra',
        detail: 'Los equipos quedan disponibles en Stock al instante, vinculados a esta compra y a su proveedor.',
      },
    ],
  },
  {
    id: 'compra-anular',
    category: 'Compras',
    title: 'Cómo anular una compra',
    summary: 'Da de baja todos los equipos que esa compra había generado.',
    steps: [
      {
        title: 'Entrá a Compras y ubicá la compra en el listado',
        detail: '',
      },
      {
        title: 'Hacé clic en "Anular"',
        detail: 'Solo la ve el dueño de la cuenta, y solo en compras con estado "Activa".',
      },
      {
        title: 'Confirmá la anulación',
        detail: 'Todos los equipos que esa compra había generado en Stock pasan a estado "Anulado".',
      },
    ],
  },

  // ─── RESERVAS ──────────────────────────────────────────────
  {
    id: 'reserva-nueva',
    category: 'Reservas',
    title: 'Cómo reservar un equipo para un cliente',
    summary: 'Apartá un equipo del stock disponible sin venderlo todavía.',
    steps: [
      {
        title: 'Entrá a Reservas y hacé clic en "+ Nueva Reserva"',
        detail: 'También disponible como acceso rápido desde el Dashboard.',
      },
      {
        title: 'Elegí si el cliente es Consumidor Final o un cliente registrado',
        detail: '',
      },
      {
        title: 'Completá los datos de la reserva',
        detail: 'Equipo a reservar, y los datos de contacto del cliente.',
      },
      {
        title: 'Guardá la reserva',
        detail: 'El equipo aparece en el Dashboard (pestaña Operativo) dentro de "Reservas activas" hasta que se retire o se cancele.',
      },
    ],
    tips: [
      'Cuando el cliente venga a retirar el equipo reservado, registrá la venta normalmente desde Ventas eligiendo ese mismo ítem de stock.',
    ],
  },
  {
    id: 'reserva-cancelar',
    category: 'Reservas',
    title: 'Cómo cancelar una reserva',
    summary: 'Libera el equipo para que vuelva a estar disponible.',
    steps: [
      {
        title: 'Entrá a Reservas y ubicá la reserva activa',
        detail: '',
      },
      {
        title: 'Hacé clic en "Cancelar"',
        detail: 'Esta opción solo la ve el dueño de la cuenta.',
      },
    ],
  },

  // ─── CAJAS ─────────────────────────────────────────────────
  {
    id: 'caja-movimiento',
    category: 'Cajas',
    title: 'Cómo registrar un movimiento de caja',
    summary: 'Ingresos y egresos manuales, en la moneda y método que corresponda.',
    steps: [
      {
        title: 'Entrá a Cajas y hacé clic en "+ Nuevo Movimiento"',
        detail: 'También disponible como acceso rápido desde el Dashboard.',
      },
      {
        title: 'Elegí en qué caja se registra el movimiento',
        detail: 'Cada negocio puede tener más de una caja; una queda marcada como "Principal".',
      },
      {
        title: 'Elegí el tipo (Ingreso o Egreso) y el método de pago',
        detail: 'Si elegís un método en pesos, el sistema te sugiere automáticamente el tipo de cambio del dólar blue del día.',
      },
      {
        title: 'Cargá el monto y guardá',
        detail: '',
      },
    ],
  },

  // ─── CUENTAS CORRIENTES ────────────────────────────────────
  {
    id: 'cuenta-pago',
    category: 'Cuentas Corrientes',
    title: 'Cómo registrar un cobro o pago',
    summary: 'Saldá (total o parcialmente) la deuda con un cliente o proveedor.',
    steps: [
      {
        title: 'Entrá a Cuentas Corrientes',
        detail: 'Vas a ver el saldo de cada cliente/proveedor: "A cobrar" (te deben) o "A pagar" (les debés).',
      },
      {
        title: 'Hacé clic en el botón de pago de la fila correspondiente',
        detail: 'Solo aparece si esa cuenta tiene saldo distinto de cero.',
      },
      {
        title: 'El monto se completa solo con el saldo pendiente',
        detail: 'Podés ajustarlo si es un pago parcial. Elegí el método de pago.',
      },
      {
        title: 'Confirmá — el saldo se actualiza al instante',
        detail: '',
      },
    ],
  },

  // ─── SERVICIO TÉCNICO ──────────────────────────────────────
  {
    id: 'servicio-nuevo',
    category: 'Servicio Técnico',
    title: 'Cómo dar de alta una orden de servicio',
    summary: 'Registrá el ingreso de un equipo para reparación.',
    steps: [
      {
        title: 'Entrá a Serv. Técnico y hacé clic en "+ Nuevo SV"',
        detail: '',
      },
      {
        title: 'Completá los datos del cliente y del equipo',
        detail:
          'Nombre, WhatsApp, modelo del dispositivo (elegilo del listado) e IMEI/serie (podés escanearlo). Si el modelo no está en la lista, el dueño de la cuenta puede agregarlo al toque con "+ Agregar modelo nuevo"; si no sos el dueño, pedile que lo cargue en Configuración → Modelos.',
      },
      {
        title: 'Cargá el precio a cobrarle al cliente y el costo del técnico',
        detail: '',
      },
      {
        title: 'Guardá la orden',
        detail: 'Entra al tablero en estado "Abierto".',
      },
    ],
  },
  {
    id: 'servicio-avanzar',
    category: 'Servicio Técnico',
    title: 'Cómo avanzar el estado de una orden',
    summary: 'De "Abierto" a "Entregado", paso a paso.',
    steps: [
      {
        title: 'Entrá a Serv. Técnico y ubicá la orden en el listado',
        detail: '',
      },
      {
        title: 'Hacé clic en el botón "→ [siguiente estado]" de esa fila',
        detail: 'El flujo es: Abierto → En reparación → Listo para entregar → Entregado. Cada clic avanza un solo paso.',
      },
    ],
  },

  // ─── DASHBOARD ─────────────────────────────────────────────
  {
    id: 'dashboard-metricas',
    category: 'Dashboard',
    title: 'Cómo interpretar las métricas del Dashboard',
    summary: 'Qué significa cada tarjeta y cada gráfico.',
    steps: [
      {
        title: 'Elegí el período arriba a la derecha',
        detail: 'Semana, mes o año — todas las tarjetas de la pestaña Financiero se recalculan para ese rango.',
      },
      {
        title: 'Pestaña Financiero',
        detail:
          'Facturación, margen bruto, ganancia neta (ya descuenta comisiones y gastos operativos), cantidad de ventas, ventas con canje, stock disponible, reservas activas y gastos.',
      },
      {
        title: 'Pestaña Operativo',
        detail: 'Estado actual de stock, reservas activas y servicios técnicos en curso — no depende del período elegido.',
      },
      {
        title: 'Gráficos de tendencia',
        detail:
          'Muestran los últimos 6 meses de facturación y de ventas con canje, siempre — independiente del período que hayas elegido arriba.',
      },
    ],
  },

  // ─── EQUIPO ────────────────────────────────────────────────
  {
    id: 'equipo-roles',
    category: 'Equipo',
    title: 'Cómo y cuándo se asignan los roles',
    summary: 'Quién es Dueño, quién es Empleado, y cómo cambiarle el rol a alguien.',
    steps: [
      {
        title: 'El primer usuario de la cuenta siempre es el Dueño',
        detail:
          'Se define una sola vez, al registrar el negocio, y no se puede reasignar después: no hay forma de convertir a otro usuario en Dueño ni de dejar de serlo vos mismo.',
      },
      {
        title: 'Todo invitado entra como Empleado',
        detail:
          'La invitación que mandás desde Mi Equipo solo lleva el email, no elegís un rol al invitar. Cuando la persona acepta el link, su cuenta se crea siempre con el rol Empleado.',
      },
      {
        title: 'Después podés cambiarle el rol desde Mi Equipo',
        detail:
          'Solo el Dueño o un Administrador pueden hacerlo. Elegís entre Administrador, Operador, Solo lectura o Empleado para cualquier miembro del equipo, menos vos mismo.',
      },
    ],
    tips: [
      'El rol de Dueño no se le puede asignar a nadie desde Mi Equipo — nace únicamente del registro inicial del negocio.',
      'El Dueño de la cuenta es el único que no se puede desactivar.',
    ],
  },
];
