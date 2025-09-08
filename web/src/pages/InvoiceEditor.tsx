import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api'
import ThirdPicker from '../components/ThirdPicker'
import AccountPicker from '../components/AccountPicker'
import { useToast } from '../components/Toast'

type Line = { itemName:string; quantity:number; unitPrice:number; discount:number; taxRate:number; accountId?:number; accountLabel?:string; }

export default function InvoiceEditor(){
  const nav = useNavigate()
  const { id } = useParams()
  const isEdit = !!id
  const [type, setType] = useState('SALE')
  const [issueDate, setIssueDate] = useState<string>(new Date().toISOString().slice(0,10))
  const [dueDate, setDueDate] = useState<string>('')
  const [thirdPartyId, setThirdPartyId] = useState<number|undefined>(undefined)
  const [status, setStatus] = useState('DRAFT')
  const [lines, setLines] = useState<Line[]>([{ itemName:'', quantity:1, unitPrice:0, discount:0, taxRate:0 }])
  const [counterAccountId, setCounterAccountId] = useState<number|undefined>(undefined)
  const { show } = useToast()

  const totals = useMemo(()=>{
    let sub=0, tax=0, total=0
    for (const l of lines){
      const base = (l.quantity * l.unitPrice) - l.discount
      const t = Math.round((base * (l.taxRate/100))*100)/100
      sub += base; tax += t; total += (base+t)
    }
    return { subtotal: Number(sub.toFixed(2)), taxTotal: Number(tax.toFixed(2)), total: Number(total.toFixed(2)) }
  },[lines])

  useEffect(()=>{
    if (!isEdit) return
    api.invoices.get(Number(id)).then((inv:any)=>{
      setType(inv.type); setIssueDate(inv.issueDate); setDueDate(inv.dueDate||''); setThirdPartyId(inv.thirdPartyId); setStatus(inv.status)
      setLines(inv.lines.map((l:any)=> ({ itemName:l.itemName, quantity:l.quantity, unitPrice:l.unitPrice, discount:l.discount, taxRate:l.taxRate, accountId:l.accountId })))
    })
  },[id, isEdit])

  const setLine = (idx:number, patch: Partial<Line>) => setLines(prev => prev.map((l,i)=> i===idx? { ...l, ...patch }: l))
  const addLine = () => setLines(prev => [...prev, { itemName:'', quantity:1, unitPrice:0, discount:0, taxRate:0 }])
  const delLine = (idx:number) => setLines(prev => prev.filter((_,i)=> i!==idx))

  const save = async () => {
    if (!thirdPartyId) { show('Seleccione tercero','error'); return }
    if (lines.length===0) { show('Agregue líneas','error'); return }
    for (const [i,l] of lines.entries()){
      if (!l.accountId) { show(`Línea ${i+1} sin cuenta`, 'error'); return }
    }
    try{
      if (isEdit){
        await api.invoices.update(Number(id), { issueDate, dueDate: dueDate||null, currency:'COP', lines })
      }else{
        await api.invoices.create({ type, thirdPartyId, issueDate, dueDate: dueDate||null, currency:'COP', lines })
      }
      show('Factura guardada','success'); nav('/invoices')
    }catch(e:any){ show(e.message||'Error','error') }
  }

  const approve = async () => {
    if (!isEdit) return
    if (!counterAccountId){ show('Seleccione cuenta contrapartida (CxC/CxP)','error'); return }
    if (!confirm('¿Aprobar factura y contabilizar?')) return
    try{ await api.invoices.approve(Number(id), { counterAccountId, postingDate: issueDate }); show('Factura aprobada','success'); nav('/invoices') }catch(e:any){ show(e.message||'Error','error') }
  }

  const cancel = async () => {
    if (!isEdit) return
    if (!confirm('¿Cancelar factura en borrador?')) return
    try{ await api.invoices.cancel(Number(id)); show('Factura cancelada','success'); nav('/invoices') }catch(e:any){ show(e.message||'Error','error') }
  }

  return (
    <div className="card">
      <div className="card-header flex items-center justify-between">
        <div className="flex items-center gap-2">
          <span>{isEdit? 'Editar factura' : 'Nueva factura'}</span>
          {isEdit && <span className={`text-xs px-2 py-1 rounded-full ${status==='DRAFT'?'bg-yellow-100 text-yellow-800': status==='APPROVED'?'bg-green-100 text-green-800':'bg-slate-100 text-slate-700'}`}>{status}</span>}
        </div>
        <div className="flex items-center gap-2">
          <button className="btn btn-secondary" onClick={()=>nav('/invoices')}>Volver</button>
          {(!isEdit || status==='DRAFT') && <button className="btn btn-primary" onClick={save}>Guardar</button>}
          {isEdit && status==='DRAFT' && <button className="btn btn-primary" onClick={approve}>Aprobar</button>}
          {isEdit && <a className="btn btn-secondary" href={`/api/invoices/${id}/pdf`} target="_blank" rel="noreferrer">PDF</a>}
          {isEdit && status==='DRAFT' && <button className="btn btn-secondary" onClick={cancel}>Cancelar</button>}
        </div>
      </div>
      <div className="card-body space-y-4">
        {!isEdit && (
          <div className="grid grid-cols-3 gap-3">
            <div><div className="label">Tipo</div><select className="input" value={type} onChange={e=>setType(e.target.value)}><option value="SALE">Venta</option><option value="PURCHASE">Compra</option></select></div>
          </div>
        )}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-3">
          <div><div className="label">Fecha emisión</div><input className="input" type="date" value={issueDate} onChange={e=>setIssueDate(e.target.value)} /></div>
          <div><div className="label">Vence</div><input className="input" type="date" value={dueDate} onChange={e=>setDueDate(e.target.value)} /></div>
          <div className="md:col-span-2"><div className="label">Tercero</div><ThirdPicker value={thirdPartyId} onChange={t=> setThirdPartyId(t?.id)} /></div>
        </div>
        <div className="border rounded-md">
          <div className="grid grid-cols-12 bg-slate-100 px-2 py-2 text-sm font-medium">
            <div className="col-span-3">Ítem</div>
            <div className="col-span-2 text-right">Cantidad</div>
            <div className="col-span-2 text-right">Valor</div>
            <div className="col-span-1 text-right">Desc.</div>
            <div className="col-span-1 text-right">IVA%</div>
            <div className="col-span-2">Cuenta</div>
            <div className="col-span-1 text-right">Total</div>
          </div>
          {lines.map((l, i)=> (
            <div key={i} className="grid grid-cols-12 gap-2 px-2 py-2 items-center border-t">
              <div className="col-span-3"><input className="input" value={l.itemName} onChange={e=> setLine(i,{ itemName:e.target.value })} /></div>
              <div className="col-span-2"><input type="number" step="0.01" className="input text-right" value={l.quantity} onChange={e=> setLine(i,{ quantity: Number(e.target.value) })} /></div>
              <div className="col-span-2"><input type="number" step="0.01" className="input text-right" value={l.unitPrice} onChange={e=> setLine(i,{ unitPrice: Number(e.target.value) })} /></div>
              <div className="col-span-1"><input type="number" step="0.01" className="input text-right" value={l.discount} onChange={e=> setLine(i,{ discount: Number(e.target.value) })} /></div>
              <div className="col-span-1"><input type="number" step="0.01" className="input text-right" value={l.taxRate} onChange={e=> setLine(i,{ taxRate: Number(e.target.value) })} /></div>
              <div className="col-span-2"><AccountPicker value={l.accountId} onChange={a=> setLine(i,{ accountId: a?.id, accountLabel: a? `${a.code} - ${a.name}`: '' })} /></div>
              <div className="col-span-1 text-right font-mono">{(((l.quantity*l.unitPrice)-l.discount) + Math.round(((l.quantity*l.unitPrice)-l.discount)*(l.taxRate/100)*100)/100).toFixed(2)}</div>
              <div className="col-span-12 text-right"><button className="btn btn-secondary" onClick={()=> delLine(i)}>Eliminar línea</button></div>
            </div>
          ))}
          <div className="flex items-center justify-between px-2 py-2 border-t">
            <button className="btn btn-secondary" onClick={addLine}>Añadir línea</button>
            <div className="text-sm space-x-3"><span>Sub: <span className="font-mono">{totals.subtotal.toFixed(2)}</span></span><span>IVA: <span className="font-mono">{totals.taxTotal.toFixed(2)}</span></span><span>Total: <span className="font-mono">{totals.total.toFixed(2)}</span></span></div>
          </div>
        </div>
        {isEdit && status==='DRAFT' && (
          <div className="grid grid-cols-2 gap-3">
            <div>
              <div className="label">Cuenta contrapartida (CxC/CxP)</div>
              <AccountPicker value={counterAccountId} onChange={a=> setCounterAccountId(a?.id)} />
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
