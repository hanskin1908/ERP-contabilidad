import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api'
import { setToken } from '../auth'

export default function Login() {
  const nav = useNavigate()
  const [email, setEmail] = useState('admin@demo.com')
  const [password, setPassword] = useState('Admin123!')
  const [error, setError] = useState<string>('')

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    try {
      const res = await api.login(email, password)
      setToken(res.token)
      nav('/accounts')
    } catch (err: any) {
      setError(err.message ?? 'Error')
    }
  }

  return (
    <div className="flex items-center justify-center min-h-[70vh]">
      <form onSubmit={onSubmit} className="card w-full max-w-md">
        <div className="card-header">Iniciar sesión</div>
        <div className="card-body space-y-3">
          <div>
            <div className="label">Email</div>
            <input className="input" value={email} onChange={e=>setEmail(e.target.value)} />
          </div>
          <div>
            <div className="label">Password</div>
            <input className="input" type="password" value={password} onChange={e=>setPassword(e.target.value)} />
          </div>
          {error && <div className="text-red-600 text-sm">{error}</div>}
          <div className="pt-1">
            <button type="submit" className="btn btn-primary w-full">Entrar</button>
          </div>
        </div>
      </form>
    </div>
  )
}
