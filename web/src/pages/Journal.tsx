import { useEffect, useState } from 'react'
import { api } from '../api'
import { Link } from 'react-router-dom'
import { useToast } from '../components/Toast'

export default function Journal(){
  const [list, setList] = useState<any[]>([])
  const [msg, setMsg] = useState('')

  const load = async ()=>{
    const res = await api.journal.list('2025-01-01','2025-12-31')
    const items = Array.isArray(res) ? res : (res.value ?? [])
    setList(items)
  }
  useEffect(()=>{ load() }, [])

  const { show } = useToast()
  const publish = async (id:number) => {
    if (!confirm('¿Publicar este asiento? No podrá editarse después.')) return
    try { await api.journal.post(id); await load(); show('Asiento publicado','success') } catch (e:any) { show(e.message||'Error','error') }
  }

  const create = async ()=>{
    setMsg('')
    try {
      const payload = { type:'DIARIO', date:'2025-01-31', description:'Asiento demo', lines:[
        { accountId: 1, debit: 10000, credit: 0, description: 'Caja' },
        { accountId: 2, debit: 0, credit: 10000, description: 'Ingresos' }
      ]}
      await api.journal.create(payload)
      await load(); setMsg('Asiento creado')
    } catch (err:any) { setMsg(err.message) }
  }

  return (
    <div className="card">
      <div className="card-header flex items-center justify-between">
        <span>Asientos</span>
        <div className="flex items-center gap-2">
          <button className="btn btn-secondary" onClick={create}>Crear asiento demo</button>
          <Link className="btn btn-primary" to="/journal/new">Nuevo asiento</Link>
        </div>
      </div>
      <div className="card-body">
        {msg && <div className="mb-3 text-slate-700 text-sm">{msg}</div>}
        <div className="overflow-auto">
          <table className="table">
            <thead><tr><th>#</th><th>Fecha</th><th>Tipo</th><th>Estado</th><th>Débito</th><th>Crédito</th><th className="text-right">Acciones</th></tr></thead>
            <tbody>
              {list.map(j => (
                <tr key={j.id}>
                  <td className="font-mono">{j.number}</td>
                  <td>{j.date}</td>
                  <td>{j.type}</td>
                  <td>{j.status}</td>
                  <td>{j.totalDebit}</td>
                  <td>{j.totalCredit}</td>
                  <td className="text-right space-x-2">
                    <Link className="btn btn-secondary" to={`/journal/${j.id}`}>Editar</Link>
                    {j.status === 'DRAFT' && <button className="btn btn-primary" onClick={()=> publish(j.id)}>Publicar</button>}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )}
