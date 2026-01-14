import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useLogin, useRegister, useGuestLogin } from '../api/hooks/useAuth';

export default function AuthPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [isLogin, setIsLogin] = useState(true);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [error, setError] = useState('');

  const loginMutation = useLogin();
  const registerMutation = useRegister();
  const guestMutation = useGuestLogin();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    try {
      if (isLogin) {
        await loginMutation.mutateAsync({ email, password });
      } else {
        await registerMutation.mutateAsync({ email, password, firstName, lastName });
      }
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.message || t('common.error'));
    }
  };

  const handleGuestLogin = async () => {
    try {
      await guestMutation.mutateAsync();
      navigate('/onboarding');
    } catch (err: any) {
      setError(err.response?.data?.message || t('common.error'));
    }
  };

  const isLoading = loginMutation.isPending || registerMutation.isPending || guestMutation.isPending;

  return (
    <div className="max-w-md mx-auto">
      <div className="bg-white p-8 rounded-lg shadow-sm">
        <h1 className="text-2xl font-bold text-center mb-6">
          {isLogin ? t('auth.login') : t('auth.register')}
        </h1>

        {error && (
          <div className="bg-red-50 text-red-600 p-3 rounded-lg mb-4">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          {!isLogin && (
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  {t('auth.firstName')}
                </label>
                <input
                  type="text"
                  value={firstName}
                  onChange={(e) => setFirstName(e.target.value)}
                  className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  {t('auth.lastName')}
                </label>
                <input
                  type="text"
                  value={lastName}
                  onChange={(e) => setLastName(e.target.value)}
                  className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>
            </div>
          )}

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('auth.email')}
            </label>
            <input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('auth.password')}
            </label>
            <input
              type="password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          <button
            type="submit"
            disabled={isLoading}
            className="w-full bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50"
          >
            {isLoading ? t('common.loading') : (isLogin ? t('auth.login') : t('auth.register'))}
          </button>
        </form>

        <div className="mt-4 text-center">
          <button
            onClick={() => setIsLogin(!isLogin)}
            className="text-blue-600 hover:underline text-sm"
          >
            {isLogin ? t('auth.noAccount') : t('auth.hasAccount')}
          </button>
        </div>

        <div className="mt-6">
          <div className="relative">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-gray-200"></div>
            </div>
            <div className="relative flex justify-center text-sm">
              <span className="px-2 bg-white text-gray-500">{t('auth.orContinueWith')}</span>
            </div>
          </div>

          <div className="mt-4 space-y-2">
            <button
              onClick={handleGuestLogin}
              disabled={isLoading}
              className="w-full border border-gray-300 py-2 rounded-lg hover:bg-gray-50 disabled:opacity-50"
            >
              {t('auth.continueAsGuest')}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
