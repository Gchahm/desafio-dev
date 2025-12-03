import React from 'react';

export interface MoneyProps {
    value?: number | null;
    locale?: string;
    currency?: string;
    className?: string;
    fallback?: React.ReactNode;
    signDisplay?: 'auto' | 'always' | 'never' | 'exceptZero';
}

export const Money: React.FC<MoneyProps> = ({
    value,
    locale = 'pt-BR',
    currency = 'BRL',
    className,
    fallback = '-',
    signDisplay = 'auto',
}) => {
    if (value === null || value === undefined || Number.isNaN(value)) {
        return <span className={className}>{fallback}</span>;
    }

    const formatter = new Intl.NumberFormat(locale, {
        style: 'currency',
        currency,
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
        signDisplay,
    } as Intl.NumberFormatOptions as any);

    return <span className={className}>{formatter.format(value)}</span>;
};
