import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '../stores/authStore';
import { useUiStore } from '../stores/uiStore';

interface LayoutProps {
  children: React.ReactNode;
}

export default function Layout({ children }: LayoutProps) {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const { isAuthenticated, isPsychologist, isAdmin, user, logout } = useAuthStore();
  const { language, setLanguage } = useUiStore();

  const handleLanguageChange = (lang: 'ru' | 'tj') => {
    setLanguage(lang);
    i18n.changeLanguage(lang);
  };

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center space-x-8">
              <Link to="/" className="text-xl font-semibold text-blue-600">
                PsychSupport
              </Link>
              <div className="hidden md:flex space-x-4">
                <Link to="/" className="text-gray-700 hover:text-blue-600 px-3 py-2">
                  {t('nav.home')}
                </Link>
                <Link to="/psychologists" className="text-gray-700 hover:text-blue-600 px-3 py-2">
                  {t('nav.psychologists')}
                </Link>
              </div>
            </div>

            <div className="flex items-center space-x-4">
              <div className="flex space-x-1">
                <button
                  onClick={() => handleLanguageChange('ru')}
                  className={`px-2 py-1 text-sm rounded ${
                    language === 'ru' ? 'bg-blue-600 text-white' : 'text-gray-600 hover:bg-gray-100'
                  }`}
                >
                  RU
                </button>
                <button
                  onClick={() => handleLanguageChange('tj')}
                  className={`px-2 py-1 text-sm rounded ${
                    language === 'tj' ? 'bg-blue-600 text-white' : 'text-gray-600 hover:bg-gray-100'
                  }`}
                >
                  TJ
                </button>
              </div>

              {isAuthenticated ? (
                <div className="flex items-center space-x-4">
                  {isPsychologist && (
                    <Link to="/dashboard" className="text-gray-700 hover:text-blue-600">
                      {t('nav.dashboard')}
                    </Link>
                  )}
                  {isAdmin && (
                    <Link to="/admin" className="text-gray-700 hover:text-blue-600">
                      {t('nav.admin')}
                    </Link>
                  )}
                  {!isPsychologist && (
                    <Link to="/my-sessions" className="text-gray-700 hover:text-blue-600">
                      {t('sessions.title')}
                    </Link>
                  )}
                  <span className="text-gray-600 text-sm">
                    {user?.firstName || user?.email}
                  </span>
                  <button
                    onClick={handleLogout}
                    className="text-gray-600 hover:text-red-600 text-sm"
                  >
                    {t('auth.logout')}
                  </button>
                </div>
              ) : (
                <Link
                  to="/auth"
                  className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
                >
                  {t('auth.login')}
                </Link>
              )}
            </div>
          </div>
        </div>
      </nav>

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {children}
      </main>

      <footer className="bg-white border-t mt-auto">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <div className="flex justify-between items-center text-sm text-gray-500">
            <span>© 2025 PsychSupport. {t('footer.rights')}</span>
            <div className="space-x-4">
              <a href="#" className="hover:text-gray-700">{t('footer.privacy')}</a>
              <a href="#" className="hover:text-gray-700">{t('footer.terms')}</a>
            </div>
          </div>
        </div>
      </footer>
    </div>
  );
}
