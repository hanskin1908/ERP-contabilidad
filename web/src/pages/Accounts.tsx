import { useEffect, useState } from 'react'
import { api } from '../api'

type Account = { id:number; code:string; name:string; level:number; nature:'D'|'C'; parentId?:number; isPostable:boolean; isActive:boolean }

export default function Accounts() {
  const [list, setList] = useState<Account[]>([])
  const [form, setForm] = useState({ code:'', name:'', level:3, nature:'D' as 'D'|'C', isPostable:true })
  const [msg, setMsg] = useState('')

  const load = async () => {
    const res = await api.accounts.list()
    const items = Array.isArray(res) ? res : (res.value ?? [])
    setList(items)
  }
  useEffect(()=>{ load() }, [])

  const create = async (e: React.FormEvent) => {
    e.preventDefault(); setMsg('')
    try {
      await api.accounts.create(form)
      setForm({ code:'', name:'', level:3, nature:'D', isPostable:true })
      await load(); setMsg('Cuenta creada')
    } catch (err:any) { setMsg(err.message) }
  }

  return (
    <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
      <div className="card">
        <div className="card-header">Catálogo de cuentas</div>
        <div className="card-body">
          <div className="overflow-auto">
            <table className="table">
              <thead><tr><th>Código</th><th>Nombre</th><th>Naturaleza</th><th>Posteable</th></tr></thead>
              <tbody>
                {list.map(a => (
                  <tr key={a.id}><td className="font-mono">{a.code}</td><td>{a.name}</td><td>{a.nature}</td><td>{a.isPostable? 'Sí':'No'}</td></tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
      <div className="card">
        <div className="card-header">Nueva cuenta (admin)</div>
        <div className="card-body">
          <form onSubmit={create} className="space-y-3">
            <div>
              <div className="label">Código</div>
              <input className="input" placeholder="1105" value={form.code} onChange={e=>setForm({...form, code:e.target.value})} required />
            </div>
            <div>
              <div className="label">Nombre</div>
              <input className="input" placeholder="Caja" value={form.name} onChange={e=>setForm({...form, name:e.target.value})} required />
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <div className="label">Naturaleza</div>
                <select className="input" value={form.nature} onChange={e=>setForm({...form, nature:e.target.value as any})}>
                  <option value="D">Débito</option>
                  <option value="C">Crédito</option>
                </select>
              </div>
              <div>
                <div className="label">Nivel</div>
                <input className="input" type="number" value={form.level} onChange={e=>setForm({...form, level:parseInt(e.target.value)})} />
              </div>
            </div>
            <label className="inline-flex items-center gap-2">
              <input type="checkbox" checked={form.isPostable} onChange={e=>setForm({...form, isPostable:e.target.checked})} /> Posteable
            </label>
            <div className="pt-1">
              <button className="btn btn-primary" type="submit">Crear</button>
              {msg && <span className="ml-3 text-sm text-slate-600">{msg}</span>}
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}
