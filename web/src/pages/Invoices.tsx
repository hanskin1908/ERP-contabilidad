import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../api'

export default function Invoices(){
  const [list, setList] = useState<any[]>([])
  const [type, setType] = useState('')
  const [status, setStatus] = useState('')
  const [from, setFrom] = useState<string>('')
  const [to, setTo] = useState<string>('')

  const load = async ()=>{
    const res = await api.invoices.list(type||undefined, status||undefined, from||undefined, to||undefined)
    const items = Array.isArray(res) ? res : (res.value ?? [])
    setList(items)
  }
  useEffect(()=>{ load() }, [])

  return (
    <div className="card">
      <div className="card-header flex items-center justify-between">
        <span>Facturas</span>
        <div className="flex items-center gap-2">
          <Link className="btn btn-primary" to="/invoices/new">Nueva factura</Link>
        </div>
      </div>
      <div className="card-body space-y-3">
        <div className="grid grid-cols-5 gap-3">
          <div><div className="label">Tipo</div><select className="input" value={type} onChange={e=>setType(e.target.value)}><option value="">Todos</option><option value="SALE">Venta</option><option value="PURCHASE">Compra</option></select></div>
          <div><div className="label">Estado</div><select className="input" value={status} onChange={e=>setStatus(e.target.value)}><option value="">Todos</option><option value="DRAFT">Borrador</option><option value="APPROVED">Aprobada</option><option value="CANCELLED">Cancelada</option></select></div>
          <div><div className="label">Desde</div><input className="input" type="date" value={from} onChange={e=>setFrom(e.target.value)} /></div>
          <div><div className="label">Hasta</div><input className="input" type="date" value={to} onChange={e=>setTo(e.target.value)} /></div>
          <div className="flex items-end"><button className="btn btn-secondary" onClick={load}>Filtrar</button></div>
        </div>
        <div className="overflow-auto">
          <table className="table">
            <thead><tr><th>Tipo</th><th>#</th><th>Fecha</th><th>Tercero</th><th>Subtotal</th><th>Impuestos</th><th>Total</th><th>Estado</th><th className="text-right">Acciones</th></tr></thead>
            <tbody>
              {list.map((i:any)=> (
                <tr key={i.id}>
                  <td>{i.type}</td>
                  <td className="font-mono">{i.number}</td>
                  <td>{i.issueDate}</td>
                  <td>{i.thirdPartyId}</td>
                  <td className="text-right">{i.subtotal?.toFixed?.(2) ?? i.subtotal}</td>
                  <td className="text-right">{i.taxTotal?.toFixed?.(2) ?? i.taxTotal}</td>
                  <td className="text-right">{i.total?.toFixed?.(2) ?? i.total}</td>
                  <td>{i.status}</td>
                  <td className="text-right space-x-2"><Link className="btn btn-secondary" to={`/invoices/${i.id}`}>Abrir</Link></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}

