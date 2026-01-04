/**
 * Tests for Input component
 */

import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@/test/utils';
import userEvent from '@testing-library/user-event';
import { Input } from './input';

describe('Input', () => {
  it('should render input element', () => {
    render(<Input aria-label="test input" />);
    
    expect(screen.getByRole('textbox')).toBeInTheDocument();
  });

  it('should display placeholder text', () => {
    render(<Input placeholder="Enter text" />);
    
    expect(screen.getByPlaceholderText('Enter text')).toBeInTheDocument();
  });

  it('should accept user input', async () => {
    const user = userEvent.setup();
    
    render(<Input aria-label="test input" />);
    
    const input = screen.getByRole('textbox');
    await user.type(input, 'Hello World');
    
    expect(input).toHaveValue('Hello World');
  });

  it('should call onChange when value changes', async () => {
    const user = userEvent.setup();
    const handleChange = vi.fn();
    
    render(<Input aria-label="test input" onChange={handleChange} />);
    
    const input = screen.getByRole('textbox');
    await user.type(input, 'a');
    
    expect(handleChange).toHaveBeenCalled();
  });

  it('should be disabled when disabled prop is true', () => {
    render(<Input aria-label="test input" disabled />);
    
    expect(screen.getByRole('textbox')).toBeDisabled();
  });

  it('should be readonly when readonly prop is true', () => {
    render(<Input aria-label="test input" readOnly />);
    
    expect(screen.getByRole('textbox')).toHaveAttribute('readonly');
  });

  it('should apply different size variants', () => {
    const { rerender } = render(<Input aria-label="test" variant="sm" />);
    expect(screen.getByRole('textbox')).toHaveClass('h-7');

    rerender(<Input aria-label="test" variant="md" />);
    expect(screen.getByRole('textbox')).toHaveClass('h-8.5');

    rerender(<Input aria-label="test" variant="lg" />);
    expect(screen.getByRole('textbox')).toHaveClass('h-10');
  });

  it('should render different input types', () => {
    const { rerender } = render(<Input type="email" aria-label="email" />);
    expect(screen.getByRole('textbox')).toHaveAttribute('type', 'email');

    rerender(<Input type="password" aria-label="password" />);
    // Password inputs don't have textbox role
    expect(screen.getByLabelText('password')).toHaveAttribute('type', 'password');
  });

  it('should apply custom className', () => {
    render(<Input aria-label="test" className="custom-class" />);
    
    expect(screen.getByRole('textbox')).toHaveClass('custom-class');
  });

  it('should focus when clicked', async () => {
    const user = userEvent.setup();
    
    render(<Input aria-label="test input" />);
    
    const input = screen.getByRole('textbox');
    await user.click(input);
    
    expect(input).toHaveFocus();
  });

  it('should handle controlled value', () => {
    const { rerender } = render(<Input aria-label="test" value="initial" onChange={() => {}} />);
    
    expect(screen.getByRole('textbox')).toHaveValue('initial');

    rerender(<Input aria-label="test" value="updated" onChange={() => {}} />);
    
    expect(screen.getByRole('textbox')).toHaveValue('updated');
  });
});
