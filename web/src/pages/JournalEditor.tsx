import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api'
import ThirdPicker from '../components/ThirdPicker'
import Modal from '../components/Modal'
import AccountPicker from '../components/AccountPicker'
import { useToast } from '../components/Toast'

type Account = { id:number; code:string; name:string }
type Third = { id:number; razonSocial:string; nit:string }

const TYPES = ['DIARIO','INGRESO','EGRESO','AJUSTE'] as const
const CATEGORIES = ['ventas','compras','servicios','gastos','impuestos','nomina','otros']

type Line = { accountId?:number; accountLabel?:string; description?:string; category?:string; debit?:number; credit?:number; thirdPartyId?:number; thirdLabel?:string }

export default function JournalEditor(){
  const nav = useNavigate()
  const { id } = useParams()
  const isEdit = !!id
  const [loading, setLoading] = useState(false)
  const [msg, setMsg] = useState('')
  const [type, setType] = useState<string>('DIARIO')
  const [date, setDate] = useState<string>(new Date().toISOString().slice(0,10))
  const [description, setDescription] = useState<string>('')
  const [thirdPartyId, setThirdPartyId] = useState<number|undefined>(undefined)
  const [status, setStatus] = useState<string>('DRAFT')
  const [lines, setLines] = useState<Line[]>([{},{ }])
  // Account search unified via AccountPicker component
  const [thirdModalOpen, setThirdModalOpen] = useState(false)
  const [thirdModalIndex, setThirdModalIndex] = useState<number | null>(null)

  const totals = useMemo(()=>{
    const d = lines.reduce((s,l)=>s+(Number(l.debit)||0),0)
    const c = lines.reduce((s,l)=>s+(Number(l.credit)||0),0)
    return { d: Number(d.toFixed(2)), c: Number(c.toFixed(2)) }
  },[lines])

  // No local accSearch; AccountPicker handles fetching and filtering

  useEffect(()=>{
    if (!isEdit) return
    setLoading(true)
    api.journal.get(Number(id)).then((j:any)=>{
      setType(j.type); setDate(j.date); setDescription(j.description||'')
      setStatus(j.status)
      setLines(j.lines.map((l:any)=>({ accountId:l.accountId, description:l.description, category:l.category, debit:l.debit, credit:l.credit, thirdPartyId:l.thirdPartyId })))
    }).finally(()=>setLoading(false))
  },[id, isEdit])

  const setLine = (idx:number, patch: Partial<Line>) => {
    setLines(prev=> prev.map((l,i)=> i===idx ? { ...l, ...patch } : l))
  }
  const addLine = () => setLines(prev=> [...prev, {}])
  const delLine = (idx:number) => setLines(prev=> prev.filter((_,i)=> i!==idx))

  const validate = (): string | null => {
    if (!date) return 'Fecha requerida'
    if (!TYPES.includes(type as any)) return 'Tipo inválido'
    if (lines.length===0) return 'Debe incluir al menos una línea'
    for(const [i,l] of lines.entries()){
      if (!l.accountId) return `Línea ${i+1}: cuenta requerida`
      const d = Number(l.debit)||0, c = Number(l.credit)||0
      if (d<0 || c<0) return `Línea ${i+1}: montos no pueden ser negativos`
      if (d===0 && c===0) return `Línea ${i+1}: débito y crédito no pueden ser 0 a la vez`
    }
    if (totals.d !== totals.c) return 'Débitos y créditos deben ser iguales'
    return null
  }

  const { show } = useToast()
  const save = async () => {
    setMsg('')
    const err = validate()
    if (err) { setMsg(err); return }
    const payload:any = {
      type,
      date,
      description,
      thirdPartyId: thirdPartyId ?? null,
      lines: lines.map(l=>({ accountId:l.accountId!, description:l.description||null, category:l.category||null, debit:Number(l.debit)||0, credit:Number(l.credit)||0, thirdPartyId: l.thirdPartyId||null }))
    }
    try{
      if (isEdit) await api.journal.update(Number(id), payload)
      else await api.journal.create(payload)
      show('Asiento guardado','success')
      nav('/journal')
    }catch(e:any){ setMsg(e.message) }
  }

  const publish = async () => {
    if (!isEdit) return
    try{
      if (!confirm('¿Publicar este asiento? No podrá editarse después.')) return
      await api.journal.post(Number(id))
      show('Asiento publicado','success')
      nav('/journal')
    }catch(e:any){ setMsg(e.message) }
  }

  return (
    <>
    <div className="card">
      <div className="card-header flex items-center justify-between">
        <div className="flex items-center gap-2">
          <span>{isEdit? 'Editar asiento' : 'Nuevo asiento'}</span>
          {isEdit && <span className={`text-xs px-2 py-1 rounded-full ${status==='DRAFT'?'bg-yellow-100 text-yellow-800':'bg-green-100 text-green-800'}`}>{status}</span>}
        </div>
        <div className="flex items-center gap-2">
          <button className="btn btn-secondary" onClick={()=>nav('/journal')}>Volver</button>
          {(!isEdit || status==='DRAFT') && <button className="btn btn-primary" onClick={save}>Guardar</button>}
          {isEdit && status==='DRAFT' && <button className="btn btn-primary" onClick={publish}>Publicar</button>}
        </div>
      </div>
      <div className="card-body space-y-4">
        {msg && <div className="text-sm text-slate-700">{msg}</div>}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-3">
          <div>
            <div className="label">Tipo</div>
            <select className="input" value={type} onChange={e=>setType(e.target.value)}>
              {TYPES.map(t=> <option key={t} value={t}>{t}</option>)}
            </select>
          </div>
          <div>
            <div className="label">Fecha</div>
            <input type="date" className="input" value={date} onChange={e=>setDate(e.target.value)} />
          </div>
          <div className="md:col-span-2">
          <div className="label">Tercero (opcional)</div>
          <ThirdPicker value={thirdPartyId} onChange={(t)=> setThirdPartyId(t?.id)} />
        </div>
        </div>
        <div>
          <div className="label">Descripción</div>
          <input className="input" value={description} onChange={e=>setDescription(e.target.value)} />
        </div>
        <div className="border rounded-md">
          <div className="grid grid-cols-12 bg-slate-100 px-2 py-2 text-sm font-medium">
            <div className="col-span-3">Cuenta</div>
            <div className="col-span-3">Detalle</div>
            <div className="col-span-2">Categoría</div>
            <div className="col-span-2">Tercero</div>
            <div className="col-span-1 text-right">Débito</div>
            <div className="col-span-1 text-right">Crédito</div>
          </div>
          {lines.map((l, i)=> (
            <div key={i} className="grid grid-cols-12 gap-2 px-2 py-2 items-center border-t">
              <div className="col-span-3">
                <AccountPicker value={l.accountId} onChange={a=> setLine(i,{ accountId: a?.id, accountLabel: a? `${a.code} - ${a.name}`: '' })} />
              </div>
              <div className="col-span-3">
                <input className="input" value={l.description ?? ''} onChange={e=> setLine(i,{ description:e.target.value })} />
              </div>
              <div className="col-span-2">
                <select className="input" value={l.category ?? ''} onChange={e=> setLine(i,{ category:e.target.value })}>
                  <option value="">(sin categoría)</option>
                  {CATEGORIES.map(c=> <option key={c} value={c}>{c}</option>)}
                </select>
              </div>
              <div className="col-span-2">
                <div className="flex gap-2">
                  <input className="input flex-1" placeholder="Sin tercero" value={l.thirdLabel ?? ''} readOnly />
                  {(isEdit && status!=='DRAFT') ? null : <button type="button" className="btn btn-secondary" onClick={()=>{ setThirdModalIndex(i); setThirdModalOpen(true) }}>Seleccionar</button>}
                  {l.thirdPartyId && (isEdit && status!=='DRAFT' ? null : <button type="button" className="btn btn-secondary" onClick={()=> setLine(i,{ thirdPartyId:undefined, thirdLabel:'' })}>Quitar</button>)}
                </div>
              </div>
              <div className="col-span-1">
                <input type="number" step="0.01" className="input text-right" value={l.debit ?? 0} onChange={e=> setLine(i,{ debit: Number(e.target.value) })} disabled={isEdit && status!=='DRAFT'} />
              </div>
              <div className="col-span-1">
                <input type="number" step="0.01" className="input text-right" value={l.credit ?? 0} onChange={e=> setLine(i,{ credit: Number(e.target.value) })} disabled={isEdit && status!=='DRAFT'} />
              </div>
              <div className="col-span-12 text-right">
                {(isEdit && status!=='DRAFT') ? null : <button className="btn btn-secondary" onClick={()=> delLine(i)}>Eliminar línea</button>}
              </div>
            </div>
          ))}
          <div className="flex items-center justify-between px-2 py-2 border-t">
            {(isEdit && status!=='DRAFT') ? <div /> : <button className="btn btn-secondary" onClick={addLine}>Añadir línea</button>}
            <div className="text-sm">Débito: <span className="font-mono">{totals.d.toFixed(2)}</span> | Crédito: <span className="font-mono">{totals.c.toFixed(2)}</span></div>
          </div>
        </div>
      </div>
    </div>
    <Modal open={thirdModalOpen} title="Seleccionar tercero" onClose={()=> setThirdModalOpen(false)}>
      <ThirdPicker value={thirdModalIndex !== null ? (lines[thirdModalIndex].thirdPartyId as number|undefined) : undefined} onChange={(t)=>{
        if (thirdModalIndex !== null){
          setLine(thirdModalIndex, { thirdPartyId: t?.id, thirdLabel: t ? `${t.razonSocial} (${t.nit})` : '' })
        }
        setThirdModalOpen(false)
      }} />
    </Modal>
    </>
  )
}
