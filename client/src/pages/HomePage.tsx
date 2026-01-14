import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

export default function HomePage() {
  const { t } = useTranslation();

  return (
    <div className="space-y-12">
      <section className="text-center py-16">
        <h1 className="text-4xl font-bold text-gray-900 mb-4">
          {t('onboarding.title')}
        </h1>
        <p className="text-xl text-gray-600 mb-8 max-w-2xl mx-auto">
          {t('onboarding.subtitle')}
        </p>
        <div className="flex justify-center space-x-4">
          <Link
            to="/onboarding"
            className="bg-blue-600 text-white px-8 py-3 rounded-lg text-lg font-medium hover:bg-blue-700"
          >
            {t('onboarding.findPsychologist')}
          </Link>
          <Link
            to="/psychologists"
            className="border border-blue-600 text-blue-600 px-8 py-3 rounded-lg text-lg font-medium hover:bg-blue-50"
          >
            {t('nav.psychologists')}
          </Link>
        </div>
      </section>

      <section className="grid md:grid-cols-3 gap-8">
        <div className="bg-white p-6 rounded-lg shadow-sm">
          <div className="text-3xl mb-4">📋</div>
          <h3 className="text-lg font-semibold mb-2">1. {t('onboarding.title')}</h3>
          <p className="text-gray-600">
            {t('onboarding.subtitle')}
          </p>
        </div>
        <div className="bg-white p-6 rounded-lg shadow-sm">
          <div className="text-3xl mb-4">🤖</div>
          <h3 className="text-lg font-semibold mb-2">2. {t('matching.results')}</h3>
          <p className="text-gray-600">
            {t('matching.matchReason')}
          </p>
        </div>
        <div className="bg-white p-6 rounded-lg shadow-sm">
          <div className="text-3xl mb-4">📅</div>
          <h3 className="text-lg font-semibold mb-2">3. {t('booking.title')}</h3>
          <p className="text-gray-600">
            {t('psychologist.selectDate')}
          </p>
        </div>
      </section>
    </div>
  );
}
