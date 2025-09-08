import { useEffect, useState } from 'react'
import { api } from '../api'

type Third = { id:number; nit:string; razonSocial:string; tipo:string; direccion?:string; email?:string; telefono?:string; isActive:boolean }

export default function ThirdParties(){
  const [list, setList] = useState<Third[]>([])
  const [search, setSearch] = useState('')
  const [typeFilter, setTypeFilter] = useState('')
  const [editing, setEditing] = useState<Third|null>(null)
  const emptyNew = { nit:'', razonSocial:'', tipo:'cliente', direccion:'', email:'', telefono:'' }
  const [formNew, setFormNew] = useState<any>(emptyNew)
  const [msg, setMsg] = useState('')

  const load = async ()=>{
    const res = await api.third.list(search || undefined, typeFilter || undefined)
    const items = Array.isArray(res) ? res : (res.value ?? [])
    setList(items)
  }
  useEffect(()=>{ load() }, [])

  const doSearch = async (e: React.FormEvent)=>{ e.preventDefault(); await load() }

  const create = async (e: React.FormEvent) => {
    e.preventDefault(); setMsg('')
    try { await api.third.create(formNew); setFormNew(emptyNew); await load(); setMsg('Creado') } catch(e:any){ setMsg(e.message) }
  }

  const update = async (e: React.FormEvent) => {
    e.preventDefault(); if (!editing) return; setMsg('')
    try { await api.third.update(editing.id, { tipo: editing.tipo, razonSocial: editing.razonSocial, direccion: editing.direccion, email: editing.email, telefono: editing.telefono, isActive: editing.isActive }); setEditing(null); await load(); setMsg('Actualizado') } catch(e:any){ setMsg(e.message) }
  }

  const toggleActive = async (t: Third) => {
    if (t.isActive) await api.third.deactivate(t.id); else await api.third.activate(t.id); await load()
  }

  return (
    <div className="grid grid-cols-1 xl:grid-cols-3 gap-6">
      <div className="xl:col-span-2 card">
        <div className="card-header">Terceros</div>
        <div className="card-body space-y-3">
          <form className="flex gap-2" onSubmit={doSearch}>
            <input className="input" placeholder="Buscar NIT o nombre" value={search} onChange={e=>setSearch(e.target.value)} />
            <select className="input" value={typeFilter} onChange={e=>setTypeFilter(e.target.value)}>
              <option value="">Todos</option>
              <option value="cliente">Cliente</option>
              <option value="proveedor">Proveedor</option>
              <option value="ambos">Ambos</option>
              <option value="otro">Otro</option>
            </select>
            <button className="btn btn-secondary" type="submit">Filtrar</button>
          </form>
          <div className="overflow-auto">
            <table className="table">
              <thead><tr><th>NIT</th><th>Razón social</th><th>Tipo</th><th>Estado</th><th></th></tr></thead>
              <tbody>
                {list.map(t => (
                  <tr key={t.id}>
                    <td className="font-mono">{t.nit}</td>
                    <td>{t.razonSocial}</td>
                    <td>{t.tipo}</td>
                    <td>{t.isActive? 'Activo':'Inactivo'}</td>
                    <td className="text-right space-x-2">
                      <button className="btn btn-secondary" onClick={()=> setEditing(t)}>Editar</button>
                      <button className="btn btn-secondary" onClick={()=> toggleActive(t)}>{t.isActive? 'Inactivar':'Activar'}</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
      <div className="card">
        <div className="card-header">{editing? 'Editar tercero' : 'Nuevo tercero'}</div>
        <div className="card-body">
          {msg && <div className="text-sm text-slate-700 mb-2">{msg}</div>}
          {editing ? (
            <form className="space-y-3" onSubmit={update}>
              <div>
                <div className="label">NIT</div>
                <input className="input" value={editing.nit} disabled />
              </div>
              <div>
                <div className="label">Razón social</div>
                <input className="input" value={editing.razonSocial} onChange={e=> setEditing({...editing, razonSocial:e.target.value})} />
              </div>
              <div>
                <div className="label">Tipo</div>
                <select className="input" value={editing.tipo} onChange={e=> setEditing({...editing, tipo:e.target.value})}>
                  <option value="cliente">Cliente</option>
                  <option value="proveedor">Proveedor</option>
                  <option value="ambos">Ambos</option>
                  <option value="otro">Otro</option>
                </select>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <div className="label">Dirección</div>
                  <input className="input" value={editing.direccion||''} onChange={e=> setEditing({...editing, direccion:e.target.value})} />
                </div>
                <div>
                  <div className="label">Email</div>
                  <input className="input" value={editing.email||''} onChange={e=> setEditing({...editing, email:e.target.value})} />
                </div>
              </div>
              <div>
                <div className="label">Teléfono</div>
                <input className="input" value={editing.telefono||''} onChange={e=> setEditing({...editing, telefono:e.target.value})} />
              </div>
              <div className="flex gap-2">
                <button className="btn btn-primary" type="submit">Guardar</button>
                <button className="btn btn-secondary" type="button" onClick={()=> setEditing(null)}>Cancelar</button>
              </div>
            </form>
          ) : (
            <form className="space-y-3" onSubmit={create}>
              <div className="grid grid-cols-3 gap-3">
                <div className="col-span-2">
                  <div className="label">NIT</div>
                  <input className="input" value={formNew.nit} onChange={e=> setFormNew({...formNew, nit:e.target.value})} required />
                </div>
                <div>
                  <div className="label">DV</div>
                  <input className="input" value={formNew.dv||''} onChange={e=> setFormNew({...formNew, dv:e.target.value})} />
                </div>
              </div>
              <div>
                <div className="label">Razón social</div>
                <input className="input" value={formNew.razonSocial} onChange={e=> setFormNew({...formNew, razonSocial:e.target.value})} required />
              </div>
              <div>
                <div className="label">Tipo</div>
                <select className="input" value={formNew.tipo} onChange={e=> setFormNew({...formNew, tipo:e.target.value})}>
                  <option value="cliente">Cliente</option>
                  <option value="proveedor">Proveedor</option>
                  <option value="ambos">Ambos</option>
                  <option value="otro">Otro</option>
                </select>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <div className="label">Dirección</div>
                  <input className="input" value={formNew.direccion||''} onChange={e=> setFormNew({...formNew, direccion:e.target.value})} />
                </div>
                <div>
                  <div className="label">Email</div>
                  <input className="input" value={formNew.email||''} onChange={e=> setFormNew({...formNew, email:e.target.value})} />
                </div>
              </div>
              <div>
                <div className="label">Teléfono</div>
                <input className="input" value={formNew.telefono||''} onChange={e=> setFormNew({...formNew, telefono:e.target.value})} />
              </div>
              <div>
                <button className="btn btn-primary" type="submit">Crear</button>
              </div>
            </form>
          )}
        </div>
      </div>
    </div>
  )
}

