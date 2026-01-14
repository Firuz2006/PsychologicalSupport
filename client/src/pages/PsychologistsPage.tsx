import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { usePsychologists, useSpecializations } from '../api/hooks/usePsychologists';
import { useUiStore } from '../stores/uiStore';

export default function PsychologistsPage() {
  const { t } = useTranslation();
  const { language } = useUiStore();
  const [filters, setFilters] = useState({
    language: '',
    workFormat: '',
    maxPrice: 0,
    specializationIds: [] as number[],
  });

  const { data: psychologists, isLoading } = usePsychologists(
    Object.entries(filters).some(([, v]) => v && (Array.isArray(v) ? v.length : v))
      ? {
          language: filters.language || undefined,
          workFormat: filters.workFormat || undefined,
          maxPrice: filters.maxPrice || undefined,
          specializationIds: filters.specializationIds.length ? filters.specializationIds : undefined,
        }
      : undefined
  );
  const { data: specializations } = useSpecializations(language);

  const toggleSpecialization = (id: number) => {
    setFilters((prev) => ({
      ...prev,
      specializationIds: prev.specializationIds.includes(id)
        ? prev.specializationIds.filter((s) => s !== id)
        : [...prev.specializationIds, id],
    }));
  };

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">{t('nav.psychologists')}</h1>

      <div className="grid md:grid-cols-4 gap-6">
        <div className="md:col-span-1">
          <div className="bg-white p-4 rounded-lg shadow-sm space-y-4">
            <h2 className="font-semibold">{t('common.filter')}</h2>

            <div>
              <label className="block text-sm text-gray-600 mb-1">{t('psychologist.languages')}</label>
              <select
                value={filters.language}
                onChange={(e) => setFilters((p) => ({ ...p, language: e.target.value }))}
                className="w-full border rounded-lg px-3 py-2"
              >
                <option value="">{t('common.all')}</option>
                <option value="ru">{t('onboarding.russian')}</option>
                <option value="tj">{t('onboarding.tajik')}</option>
              </select>
            </div>

            <div>
              <label className="block text-sm text-gray-600 mb-1">{t('psychologist.formats')}</label>
              <select
                value={filters.workFormat}
                onChange={(e) => setFilters((p) => ({ ...p, workFormat: e.target.value }))}
                className="w-full border rounded-lg px-3 py-2"
              >
                <option value="">{t('common.all')}</option>
                <option value="Online">{t('onboarding.formatOnline')}</option>
                <option value="Chat">{t('onboarding.formatChat')}</option>
              </select>
            </div>

            <div>
              <label className="block text-sm text-gray-600 mb-1">{t('psychologist.price')}</label>
              <input
                type="number"
                placeholder="Max"
                value={filters.maxPrice || ''}
                onChange={(e) => setFilters((p) => ({ ...p, maxPrice: parseInt(e.target.value) || 0 }))}
                className="w-full border rounded-lg px-3 py-2"
              />
            </div>

            <div>
              <label className="block text-sm text-gray-600 mb-2">{t('psychologist.specializations')}</label>
              <div className="space-y-1 max-h-48 overflow-y-auto">
                {specializations?.map((spec) => (
                  <label key={spec.id} className="flex items-center space-x-2 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={filters.specializationIds.includes(spec.id)}
                      onChange={() => toggleSpecialization(spec.id)}
                      className="rounded"
                    />
                    <span className="text-sm">{spec.name}</span>
                  </label>
                ))}
              </div>
            </div>
          </div>
        </div>

        <div className="md:col-span-3">
          {isLoading ? (
            <div className="text-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
            </div>
          ) : (
            <div className="grid md:grid-cols-2 gap-4">
              {psychologists?.map((psych) => (
                <div key={psych.id} className="bg-white rounded-lg shadow-sm p-4">
                  <div className="flex items-start space-x-4">
                    {psych.photoPath ? (
                      <img src={psych.photoPath} alt="" className="w-16 h-16 rounded-full object-cover" />
                    ) : (
                      <div className="w-16 h-16 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 text-xl font-semibold">
                        {psych.firstName?.charAt(0) || 'P'}
                      </div>
                    )}
                    <div className="flex-1">
                      <div className="flex justify-between">
                        <h3 className="font-semibold">
                          {psych.firstName} {psych.lastName}
                        </h3>
                        {psych.isVerified && (
                          <span className="text-green-600 text-sm">✓ {t('psychologist.verified')}</span>
                        )}
                      </div>
                      <p className="text-gray-600 text-sm">
                        {psych.experienceYears} {t('psychologist.years')}
                      </p>
                      <div className="mt-2 flex flex-wrap gap-1">
                        {psych.specializations.slice(0, 2).map((spec) => (
                          <span key={spec.id} className="px-2 py-0.5 bg-gray-100 text-gray-600 text-xs rounded">
                            {spec.name}
                          </span>
                        ))}
                      </div>
                      <div className="mt-3 flex justify-between items-center">
                        <span className="font-semibold">
                          {psych.pricePerSession} {t('psychologist.currency')}
                        </span>
                        <Link
                          to={`/psychologists/${psych.id}`}
                          className="text-blue-600 hover:underline text-sm"
                        >
                          {t('matching.viewProfile')}
                        </Link>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
